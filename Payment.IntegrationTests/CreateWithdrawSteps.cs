using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
	public class CreateWithdrawSteps
	{
		private ValueTask<Guid> _createFormId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public CreateWithdrawSteps()
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

		[Given(@"I will create withdraw form")]
		public void GivenIWillCreateWithdrawForm()
		{
		}

		[Then(@"withdraw form created")]
		public void ThenWithdrawFormCreated()
		{
			Assert.AreNotEqual(Guid.Empty, _createFormId);
		}

		[When(@"call create withdraw form api")]
		public void WhenCallCreateWithdrawFormApi()
		{
			var withdrawForm = new WithdrawFormCreateInfo
			{
				Amount = 1000M,
				BankInfoId = Guid.NewGuid(),
				UserId = "PlayerCode",
				UserName = "UserName",
				BrandId = Guid.NewGuid(),
				UserType = UserType.Player
			};

			_createFormId = _paymentServiceClient.CreateWithdrawFormAsync(withdrawForm);
		}
	}
}
