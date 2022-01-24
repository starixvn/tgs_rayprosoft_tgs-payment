using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Tgs.Payment.Configuration;
using Tgs.Payment.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/config/appsettings.json", true);
builder.Logging.AddTgsLog();

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

var app = builder.Build();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService v1"));

app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapSwagger();

app.Run();
