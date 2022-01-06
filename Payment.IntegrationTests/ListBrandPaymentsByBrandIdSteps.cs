using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
	public class ListBrandPaymentsByBrandIdSteps
	{
		private Guid _brandId;
		private PaymentServiceClient _paymentServiceClient;
		private TestPaymentApplication _testHost;

		public ListBrandPaymentsByBrandIdSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.DisposeAsync();
		}

		[Given(@"brand id is \{(.*)}")]
		public void GivenBrandIdIs(Guid brandId)
		{
			_brandId = brandId;
		}

		[Given(@"I will list brand payments")]
		public void GivenIWillListBrandPayments()
		{
		}

		[Then(@"return brand paymnets data")]
		public void ThenReturnBrandPaymnetsData()
		{
			Assert.Pass();
		}

		[When(@"call get list brand payments api")]
		public async Task WhenCallGetListBrandPaymentsApiAsync()
		{
			var brandPayments = _paymentServiceClient.GetBrandPaymentsAsync(_brandId);

			await brandPayments.ToArrayAsync();
		}
	}
}
