using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
	public class GetAllPaymentsSteps
	{
		private PaymentServiceClient _paymentServiceClient;
		private TestPaymentApplication _testHost;

		public GetAllPaymentsSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.DisposeAsync();
		}

		[Given(@"I will list all payments")]
		public void GivenIWillListAllPayments()
		{
		}

		[Then(@"return all paymnets data")]
		public void ThenReturnAllPaymnetsData()
		{
			Assert.Pass();
		}

		[When(@"call get all payments api")]
		public async Task WhenCallGetAllPaymentsApiAsync()
		{
			var payments = _paymentServiceClient.GetAllPaymentsAsync();

			await payments.ToArrayAsync();
		}
	}
}
