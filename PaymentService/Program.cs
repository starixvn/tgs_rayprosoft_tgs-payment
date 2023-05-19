using System.Text.Json;
using System.Text.Json.Serialization;
using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Payment.Configuration;
using Tgs.Payment.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("/config/appsettings.json", true)
	.AddEnvironmentVariables();

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
				"MachineName",
				() => Environment.MachineName));
		});
	});

builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers()
	.AddMvcOptions(mvcOptions => mvcOptions
		.AddWebApiToolkitFilters())
	.AddJsonOptions(options => options
		.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentService", Version = "v1" });

	c.OperationFilter<ApiDocOperationFilter>();
	c.ApplyWebApiToolkitSupport();

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
		options.ConnectionString = config.GetConnectionString("RedisConfigurationString")!;
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
	.ConfigureBankInfoIndexParsers((options, _) =>
	{
		var parsersConfig = new Dictionary<string, Dictionary<string, string>>();
		builder.Configuration.Bind("BankInfoParsers", parsersConfig);

		foreach (var p in parsersConfig)
			options.Add(p.Key, JsonSerializer.SerializeToDocument(p.Value));
	})
	.RegisterWithdrawProcessors()
	.Services
	.AddMemoryCache()
	.AddPaymentCache()
	.AddPlayerServiceClient((options, sp) =>
	{
		var config = sp.GetRequiredService<IConfiguration>();

		options.BaseUrl = config.GetValue<Uri>("Payment:PlayerServiceBaseUrl")!;
	});

builder.Services
	.AddMessageQueue()
	.AddNatsMessageQueue(config => config
		.ConfigQueueOptions((options, sp) =>
		{
			var config = sp.GetRequiredService<IConfiguration>();

			options.Url = config.GetConnectionString("Nats")!;
		})
		.AddReplyHandler(
			Tgs.Payment.MessageQueue.ProcessorTypes.SaveWithdrawThirdNoHandlerType,
			"tgs.payment.withdraw.external_id",
			"tgs.payment.withdraw.external_id")
		.AddReplyHandler(
			Tgs.Payment.MessageQueue.ProcessorTypes.TagWithdrawPaidHandlerType,
			"tgs.payment.withdraw.paid",
			"tgs.payment.withdraw.paid")
		.AddHandler(
			Tgs.Payment.Cache.MessageQueue.ProcessorTypes.PaymentCreatedEventHandlerType,
			"tgs.payment.created")
		.AddHandler(
			Tgs.Payment.Cache.MessageQueue.ProcessorTypes.PaymentMethodCreatedEventHandlerType,
			"tgs.payment-method.created")
		.AddHandler(
			Tgs.Payment.Cache.MessageQueue.ProcessorTypes.PaymentChannelCreatedEventHandlerType,
			"tgs.payment-channel.created")
		.AddHandler(
			Tgs.Payment.Cache.MessageQueue.ProcessorTypes.PaymentMethodChannelChangedEventHandlerType,
			"tgs.payment-method-channel.changed")
		.AddHandler(
			Tgs.Payment.MessageQueue.ProcessorTypes.PlayerBankInfoAddedHandlerType,
			"tgs.player.bankinfo.added",
			"tgs.player.bankinfo.added")
		.AddHandler(
			Tgs.Payment.MessageQueue.ProcessorTypes.PlayerBankInfoDeletedHandlerType,
			"tgs.player.bankinfo.deleted",
			"tgs.player.bankinfo.deleted")
		.AddReplyHandler(
			Tgs.Payment.MessageQueue.ProcessorTypes.RebuildPlayerBankInfoTagHandlerType,
			"tgs.player.bankinfo.rebuild",
			"tgs.player.bankinfo.rebuild"))
	.AddNatsGlobPatternExchange("*");

builder.Services.AddHealthChecks();

builder.Services
	.AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
		sp => new ConfigureNamedOptions<OtlpExporterOptions>(
			Options.DefaultName,
			exporterOptions =>
			{
				var config = sp.GetRequiredService<IConfiguration>();
				var nodeIp = config.GetValue<string>("NODE_IP");

				if (!string.IsNullOrWhiteSpace(nodeIp))
					exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

				config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
			}))
	.AddOpenTelemetry()
	.WithTracing(tracingBuilder => tracingBuilder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
			.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddSource("TGS.*")
		.AddAspNetCoreInstrumentation()
		.AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
		.AddHttpClientInstrumentation()
		.AddProfileViewExporter()
		.AddOtlpExporter())
	.WithMetrics(meterBuiler => meterBuiler
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
			.AddEnvironmentVariableDetector())
		.AddRuntimeInstrumentation()
		.AddAspNetCoreInstrumentation()
		.AddOtlpExporter())
	.Services
	.Configure<AspNetCoreInstrumentationOptions>(
		options => options.Filter = httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
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
