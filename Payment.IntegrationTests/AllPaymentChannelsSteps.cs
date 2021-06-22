using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
	public class AllPaymentChannelsSteps
	{
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public AllPaymentChannelsSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_ = _testHost.StartAsync();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[Given(@"I will list all payment channels")]
		public void GivenIWillListAllPaymentChannels()
		{
		}

		[Then(@"return all payment channels data")]
		public void ThenReturnAllPaymentChannelsData()
		{
			Assert.Pass();
		}

		[When(@"call get all payment channels")]
		public async Task WhenCallGetAllPaymentChannelsAsync()
		{
			var paymentChannels = _paymentServiceClient.GetAllPaymentChannelsAsync();

			await paymentChannels.ToArrayAsync();
		}
	}
}
