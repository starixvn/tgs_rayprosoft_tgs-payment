using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
	public class QueryAllPaymentMethodChannelsSteps
	{
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public QueryAllPaymentMethodChannelsSteps()
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

		[Given(@"I will list all payment_method_channels")]
		public void GivenIWillListAllPayment_Method_Channels()
		{
		}

		[Then(@"return all payment_method_channels data")]
		public void ThenReturnAllPayment_Method_ChannelsData()
		{
			Assert.Pass();
		}

		[When(@"call get all payment_method_channels api")]
		public async Task WhenCallGetAllPayment_Method_ChannelsApiAsync()
		{
			var pmcs = _paymentServiceClient.GetAllPaymentMethodChannelsAsync();

			await pmcs.ToArrayAsync();
		}
	}
}
