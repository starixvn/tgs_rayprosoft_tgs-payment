using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
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

			_paymentServiceClient = new PaymentServiceClient(
				_testHost.GetTestClient(),
				new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});
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
