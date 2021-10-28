using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Tgs.Payment;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	internal static class TestSiteHelper
	{
		public static IHost GetPaymentServiceTestHost()
			=> GetPaymentServiceTestHost(services => { });

		public static IHost GetPaymentServiceTestHost(Action<IServiceCollection> configureServices)
			=> Host.CreateDefaultBuilder(Array.Empty<string>())
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
						.UseStartup<PaymentService.Startup>()
						.ConfigureAppConfiguration((ctx, builder) =>
						{
							builder.Sources.Clear();
						})
						.UseTestServer()
						.ConfigureTestServices(services =>
						{
							services
								.AddFakeNatsMessageQueue()
								.AddSingleton(sp =>
								{
									var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
									dbContextOptionsBuilder
										.UseInMemoryDatabase(Guid.NewGuid().ToString())
										.ConfigureWarnings(x => x.Ignore(
											InMemoryEventId.TransactionIgnoredWarning));

									return new TgsContext(dbContextOptionsBuilder.Options);
								});

							services.AddSingleton(services =>
							{
								var fakeStore = Substitute.For<ITakeNumberStore>();								

								return fakeStore;
							});

							configureServices(services);
						});
				})
				.Build();
	}
}
