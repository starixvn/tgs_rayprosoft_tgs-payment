using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Payment.Configuration;
using Tgs.Payment.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("/config/appsettings.json", true)
	.AddEnvironmentVariables();

builder.Logging
	.AddTgsLog()
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
			fluentOptions.UseMessagePack();
		});
	});

builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	});

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentService", Version = "v1" });

	c.OperationFilter<ApiDocOperationFilter>();

	var xml = $"PaymentService.xml";
	var path = Path.Combine(AppContext.BaseDirectory, xml);
	if (File.Exists(path))
		c.IncludeXmlComments(path);
});

builder.Services
	.AddPaymentClient()
	.AddPayment()
	.UseRedis((options, sp) =>
	{
		var config = sp.GetRequiredService<IConfiguration>();
		options.ConnectionString = config.GetConnectionString("RedisConfigurationString");
	})
	.UseTgsPaymentRepository((sp, builder) =>
	{
		var config = sp.GetRequiredService<IConfiguration>();

		_ = builder.UseNpgsql(config.GetConnectionString("Tgs"));
	})
	.ConfigureOptions((options, sp) =>
	{
		var jsonOptions = sp.GetRequiredService<IOptions<JsonOptions>>().Value;

		options.JsonSerializerOptions = jsonOptions.JsonSerializerOptions;
	})
	.RegisterWithdrawProcessors();

builder.Services.AddHealthChecks();

builder.Services
	.AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
		sp => new ConfigureNamedOptions<OtlpExporterOptions>(
			Microsoft.Extensions.Options.Options.DefaultName,
			exporterOptions =>
			{
				var config = sp.GetRequiredService<IConfiguration>();
				var nodeIp = config.GetValue<string>("NODE_IP");

				if (!string.IsNullOrWhiteSpace(nodeIp))
					exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

				config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
			}))
	.AddOpenTelemetryTracing(
		tracingBuilder => tracingBuilder
			.SetResourceBuilder(ResourceBuilder.CreateDefault()
				.AddEnvironmentVariableDetector())
			.AddSource("Tgs.*")
			.AddSource("TGS.*")
			.AddAspNetCoreInstrumentation()
			.AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
			.AddHttpClientInstrumentation()
			.AddProfileViewExporter()
			.AddOtlpExporter())
	.Configure<AspNetCoreInstrumentationOptions>(
		options => options.Filter = httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
			&& !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
	.AddProfileViewer();

var app = builder.Build();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService v1"));

app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapSwagger();
app.MapProfileViewer();

app.Run();
