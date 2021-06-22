using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	internal static class TestSiteHelper
	{
		public static PaymentServiceClient GetPaymentServiceClient(this IHost host)
			=> new PaymentServiceClient(
				host.GetTestClient(),
				new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					Converters = {
						new JsonStringEnumConverter()
					}
				});

		public static IHost GetPaymentServiceTestHost()
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
								.AddSingleton<TgsContext>(sp =>
								{
									var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
									dbContextOptionsBuilder
										.UseInMemoryDatabase(Guid.NewGuid().ToString())
										.ConfigureWarnings(x => x.Ignore(
											InMemoryEventId.TransactionIgnoredWarning));

									return new TgsContext(dbContextOptionsBuilder.Options);
								});
						});
				})
				.Build();
	}
}
