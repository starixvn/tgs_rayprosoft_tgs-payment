using System.Text.Json;
using System.Text.Json.Serialization;
using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NATS.Client.Core;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.MessageQueue.Nats.Serializers;
using Tgs.Payment.Configuration;
using Tgs.Payment.Swagger;

var builder = WebApplication.CreateBuilder(args);
{
	_ = builder.Configuration.AddJsonFile("appsettings.json",
		optional: true,
		reloadOnChange: true);

	_ = builder.WebHost.UseUrls($"http://0.0.0.0:{builder.Configuration["HostingPort"]}");

	_ = builder.WebHost.UseSentry();

	_ = builder.Logging
		.AddOpenTelemetry(options =>
		{
			options.IncludeFormattedMessage = true;
			options.ParseStateValues = true;
			_ = options.AddFluentdForwardExporter(fluentOptions =>
			{
				var nodeIp = builder.Configuration.GetValue<string>("Otel:NodeIp")!;
				fluentOptions.Host = nodeIp;
				fluentOptions.Tag = builder.Configuration.GetValue<string>("Otel:ServiceName")!;
				fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
					"MachineName",
					() => Environment.MachineName));
			});
		});

	_ = builder.Services
		.AddProblemDetails()
		.AddRouting(options => options.LowercaseUrls = true)
		.AddControllers()
		.AddMvcOptions(mvcOptions => mvcOptions
			.AddWebApiToolkitFilters())
		.AddJsonOptions(options => options
			.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

	_ = builder.Services
		.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentService", Version = "v1" });

			c.OperationFilter<ApiDocOperationFilter>();
			c.ApplyWebApiToolkitSupport();

			var xml = $"PaymentService.xml";
			var path = Path.Combine(AppContext.BaseDirectory, xml);
			if (File.Exists(path))
				c.IncludeXmlComments(path);
		});

	_ = builder.Services
		.AddSingleton(TimeProvider.System)
		.AddPaymentClient()
		.AddPayment()
		.UseRedis((options, sp) =>
		{
			var config = sp.GetRequiredService<IConfiguration>();
			options.ConnectionString = config.GetConnectionString("RedisConfigurationString")!;
		})
		.UseTgsPaymentRepository((sp, builder) => builder
			.UseNpgsql("name=Tgs"))
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
		.AddPlayerServiceClient((options, _) => options.BaseUrl =
			builder.Configuration.GetValue<Uri>("Payment:PlayerServiceBaseUrl")!)
		.AddCashServiceClient((options, _) => options.BaseUrl =
			builder.Configuration.GetValue<Uri>("Payment:CashServiceBaseUrl")!)
		.AddPlayerBucketClient(httpBuilder => httpBuilder
			.ConfigureHttpClient(httpClient => httpClient.BaseAddress =
				builder.Configuration.GetValue<Uri>("Payment:PlayerBucketServiceBaseUrl")!))
		.AddPlayerBucketTypeClient()
		.AddVipLevelServiceClient(httpBuilder => httpBuilder
			.ConfigureHttpClient(httpClient => httpClient.BaseAddress =
				builder.Configuration.GetValue<Uri>("Payment:VipLevelServiceBaseUrl")!))
		.AddWithdrawLimitClient()
		.AddBrandServiceClient((options, _) => options.BrandCertificateBaseUrl =
			builder.Configuration.GetValue<Uri>("Payment:BrandCertificateBaseUrl")!)
		.AddAgentClient()
			.ConfigureHttpClient(httpClient => httpClient.BaseAddress =
				builder.Configuration.GetValue<Uri>("Payment:AgentServiceBaseUrl")!);

	_ = builder.Services
		.AddKeyedSingleton(
			"SupportCodes",
			(sp, _) => builder.Configuration
				.GetSection("Payment:SupportQueryBalancePaymentCodes")
				.GetChildren()
				.ToArray()
				.Select(v => v.Value!.ToLower())
				.ToHashSet())
		.AddKeyedSingleton("Nats", (sp, _) => new NatsConnection(NatsOpts.Default with
		{
			Url = builder.Configuration.GetConnectionString("Nats")!,
			SerializerRegistry = TgsNatsSerializerRegistration.Default
		}))
		.AddMessageQueue()
		.AddNatsMessageQueue(config => config
			.ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats"))
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
				"tgs.player.bankinfo.rebuild")
			.AddHandler(
				Tgs.Payment.MessageQueue.ProcessorTypes.ApproveWithdrawHandlerType,
				"tgs.payment.withdraw.status_ex",
				"tgs.payment.withdraw.auto_approve")
			.AddHandler(
				Tgs.Payment.MessageQueue.ProcessorTypes.CountBrandDepositApprovedType,
				"tgs.payment.deposit.status_ex",
				"tgs.payment.deposit.status_ex")
			.AddHandler(
				Tgs.Payment.MessageQueue.ProcessorTypes.CountBrandWithdrawPaidType,
				"tgs.payment.withdraw.status_ex",
				"tgs.payment.withdraw.status_ex")
			.AddProcessor(
				Tgs.Payment.MessageQueue.ProcessorTypes.PaymentBalanceDispatchProcessorType,
				"tgs.payment.dispatch_balance.*",
				"tgs.payment.dispatch_balance",
				processorFactory: Tgs.Payment.MessageQueue.ProcessorTypes.MakePaymentBalanceDispatchProcessorFactory(
					sp => sp.GetRequiredKeyedService<HashSet<string>>("SupportCodes"))))
		.AddNatsGlobPatternExchange("*");

	var instrumentationExtenionFilters = new[] { "html", "ico", "png", "jpg" };
	_ = builder.Services
		.AddHealthChecks()
		.Services
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
		.ConfigureResource(_ => _.AddService(builder.Configuration.GetValue<string>("Otel:ServiceName")))
		.WithTracing(tracingBuilder => tracingBuilder
			.SetResourceBuilder(ResourceBuilder.CreateDefault()
				.AddEnvironmentVariableDetector())
			.AddSource("Tgs.*")
			.AddSource("TGS.*")
			.AddSource("Valhalla.*")
			.AddAspNetCoreInstrumentation(options => options.Filter =
				httpContext => !httpContext.Request.Path.HasValue
					|| !httpContext.Request.Path.StartsWithSegments("/swagger")
					&& !httpContext.Request.Path.StartsWithSegments("/healthz")
					&& !httpContext.Request.Path.StartsWithSegments("/metrics")
					&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
					&& !instrumentationExtenionFilters.Any(
						ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
			.AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
			.AddHttpClientInstrumentation()
			.AddProfileViewExporter()
			.AddOtlpExporter(options => options.Endpoint = builder.Configuration.GetValue<Uri>("Otel:Collector")))
		.WithMetrics(meterBuiler => meterBuiler
			.SetResourceBuilder(ResourceBuilder.CreateDefault()
				.AddEnvironmentVariableDetector())
			.AddRuntimeInstrumentation()
			.AddAspNetCoreInstrumentation()
			.AddOtlpExporter())
		.Services
		.AddProfileViewer();
}

var app = builder.Build();
{
	_ = app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService v1"));

	_ = app.MapControllers();
	_ = app.MapHealthChecks("/healthz");
	_ = app.MapSwagger();
	app.MapProfileViewer();
}

SentrySdk.CaptureMessage($"Started result Tgs-PaymentService at {DateTime.Now.ToString()}");

app.Run();