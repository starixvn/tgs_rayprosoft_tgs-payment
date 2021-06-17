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
	public class GetDepositDetail
	{
		private Guid _brandId;
		private Guid _formId;
		private IHost _testHost;
		private PaymentServiceClient _paymentServiceClient;
		private DepositFormInfo _actual;

		public GetDepositDetail()
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

		[Given(@"I have a brand id \{(.*)}")]
		public void GivenIHaveABrandId(Guid brandId)
		{
			_brandId = brandId;
		}

		[Given(@"a form id \{(.*)}")]
		public void GivenAFormId(Guid formId)
		{
			_formId = formId;
		}


		[When(@"call get deposit form detail api")]
		public async Task WhenCallGetDepositFormDetailApiAsync()
		{
			using var scope = _testHost.Services.CreateScope();

			var dbContext = scope.ServiceProvider.GetRequiredService<TgsContext>();

			await dbContext.Players.AddAsync(new Player
			{
				PlayerCode = "PlayerCode"
			});

			var pmc = new PaymentMethodChannel
			{
				Id = Guid.NewGuid(),
				PaymentId = Guid.NewGuid(),
				MethodId = Guid.NewGuid(),
				ChannelId = Guid.NewGuid()
			};

			await dbContext.PaymentMethodChannels.AddAsync(pmc);

			var bp = new BrandPayment
			{
				Id = Guid.NewGuid(),
				PaymentMethodChannelId = pmc.Id
			};
			await dbContext.BrandPayments.AddAsync(bp);

			await dbContext.DepositForms.AddAsync(new DepositForm
			{
				Id = _formId,
				BrandId = _brandId,
				PlayerCode = "PlayerCode",
				BrandPaymentId = bp.Id,
				Amount = 1000M,
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now
			});

			await dbContext.SaveChangesAsync();

			_actual = await _paymentServiceClient.GetDepositFormAsync(_brandId, _formId);
		}

		[Then(@"return detail data")]
		public void ThenReturnDetailData()
		{
			Assert.IsNotNull(_actual);
		}

	}
}
