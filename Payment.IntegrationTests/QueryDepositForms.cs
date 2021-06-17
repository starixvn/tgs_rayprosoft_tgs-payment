using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	public class QueryDepositForms
	{
		private readonly PaymentServiceClient _paymentServiceClient;
		private readonly IHost _testHost;
		private PagingResponse<Tgs.Payment.Clients.DepositFormInfo> _actual;

		public QueryDepositForms()
		{
			_testHost = Host.CreateDefaultBuilder(Array.Empty<string>())
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
						.UseStartup<PaymentService.Startup>()
						.UseTestServer()
						.ConfigureTestServices(services =>
						{
							services
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
			_ = _testHost.StartAsync();

			_paymentServiceClient = new PaymentServiceClient(
				_testHost.GetTestClient(),
				new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});
		}

		[AfterScenario]
		public void CleanUp()
		{
			_ = _testHost.StopAsync();
		}

		[Given(@"I Query deposit forms")]
		public void GivenIQueryDepositForms()
		{
		}

		[Then(@"return deposit forms list")]
		public void ThenReturnDepositFormsList()
		{
			Assert.AreEqual(0, _actual.TotalCount);
			Assert.AreEqual(-1, _actual.Rows);
			Assert.NotNull(_actual.Items);
		}

		[When(@"call get deposit forms api")]
		public async Task WhenCallGetDepositFormsApiAsync()
		{
			var conditions = new Tgs.Payment.Clients.DepositFormQueryConditions
			{
				BrandId = Guid.NewGuid()
			};
			var paging = new Paging
			{
				ShowAll = true
			};

			_actual = await _paymentServiceClient.QueryDepositFormsAsync(conditions, paging);
		}
	}
}
