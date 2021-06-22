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
	public class AllPaymentMethodsSteps
	{
		private IHost _testHost;
		private PaymentServiceClient _paymentServiceClient;

		public AllPaymentMethodsSteps()
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

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.StopAsync();
		}

		[Given(@"I will list all payment methods")]
		public void GivenIWillListAllPaymentMethods()
		{
		}

		[Then(@"return all payment methods data")]
		public void ThenReturnAllPaymentMethodsData()
		{
			Assert.Pass();
		}

		[When(@"call get all payment methods api")]
		public async Task WhenCallGetAllPaymentMethodsApiAsync()
		{
			var paymentMethods = _paymentServiceClient.GetAllPaymentMethodsAsync();

			await paymentMethods.ToArrayAsync();
		}
	}
}
