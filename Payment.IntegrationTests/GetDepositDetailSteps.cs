using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Scenario = "Get deposit detail")]
	public class GetDepositDetailSteps
	{
		private DepositFormInfo _actual;
		private Guid _brandId;
		private Guid _formId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public GetDepositDetailSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();
			_ = _testHost.StartAsync();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.StopAsync();
		}

		[Given(@"a form id \{(.*)}")]
		public void GivenAFormId(Guid formId)
		{
			_formId = formId;
		}

		[Given(@"I have a brand id \{(.*)}")]
		public void GivenIHaveABrandId(Guid brandId)
		{
			_brandId = brandId;
		}

		[Then(@"return detail data")]
		public void ThenReturnDetailData()
		{
			Assert.IsNotNull(_actual);
		}

		[When(@"call get deposit form detail api")]
		public async Task WhenCallGetDepositFormDetailApiAsync()
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

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
	}
}
