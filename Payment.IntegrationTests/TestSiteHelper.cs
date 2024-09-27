using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using Tgs.Payment;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

internal static class TestSiteHelper
{
	public static TestPaymentApplication GetPaymentServiceTestHost()
		=> GetPaymentServiceTestHost(services => { });

	public static TestPaymentApplication GetPaymentServiceTestHost(Action<IServiceCollection> configureServices)
	{
		var inMemorySettings = new Dictionary<string, string> {
								{"Otel:ServiceName", "Tgs"},
								{"Otel:Collector", "http://localhost"},
							};

		return new(webBuilder => _ = webBuilder
			.UseSentry("")
			.ConfigureAppConfiguration((ctx, builder) =>
			{
				builder.Sources.Clear();
				_ = builder.AddInMemoryCollection(inMemorySettings!);
			})
			.ConfigureServices(services =>
			{
				_ = services
					.AddFakeNatsMessageQueue()
					.Replace(ServiceDescriptor.Singleton(sp =>
					{
						var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
						_ = dbContextOptionsBuilder
							.UseInMemoryDatabase(Guid.NewGuid().ToString())
							.ConfigureWarnings(x => x.Ignore(
								InMemoryEventId.TransactionIgnoredWarning));

						return new TgsContext(dbContextOptionsBuilder.Options);
					}));

				_ = services.AddSingleton(services =>
				{
					var fakeStore = Substitute.For<ITakeNumberStore>();

					return fakeStore;
				});

				_ = services.AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
					sp => new ConfigureNamedOptions<OtlpExporterOptions>(
						Options.DefaultName,
						exporterOptions => exporterOptions.TimeoutMilliseconds = 0));

				configureServices(services);
			}));
		}
}
