using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Scenario = "Get deposit detail by form id")]
	public class GetDepositDetailByFormIdSteps
	{
		private DepositFormInfo _actual;
		private Guid _formId;
		private PaymentServiceClient _paymentServiceClient;
		private TestPaymentApplication _testHost;

		public GetDepositDetailByFormIdSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.DisposeAsync();
		}

		[Given(@"I have a form id \{(.*)}")]
		public void GivenIHaveAFormIdDCAA_ADBB(Guid formId)
		{
			_formId = formId;
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
				PlayerCode = "PlayerCode",
				DefaultWalletCurrencyCode = "CNY",
				Password = Array.Empty<byte>(),
				Salt = Array.Empty<byte>(),
				Username = "player",
				WithdrawPassword = Array.Empty<byte>(),
				WithdrawSalt = Array.Empty<byte>()
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
				PaymentMethodChannelId = pmc.Id,
				ChannelName = "channel",
				CreatedByUsername = "System",
				ExtendedData = "{}",
				UpdatedByUsername = "System"
			};
			await dbContext.BrandPayments.AddAsync(bp);

			await dbContext.DepositForms.AddAsync(new DepositForm
			{
				Id = _formId,
				BrandId = Guid.NewGuid(),
				PlayerCode = "PlayerCode",
				BrandPaymentId = bp.Id,
				Amount = 1000M,
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now
			});

			await dbContext.SaveChangesAsync();

			_actual = await _paymentServiceClient.GetDepositFormAsync(_formId);
		}
	}
}
