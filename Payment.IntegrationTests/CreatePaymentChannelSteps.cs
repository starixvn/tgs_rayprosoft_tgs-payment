using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Scenario = "Create a payment channel")]
	public class CreatePaymentChannelSteps
	{
		private readonly PaymentServiceClient _paymentServiceClient;
		private readonly IHost _testHost;
		private string _paymentChannelCode;

		public CreatePaymentChannelSteps()
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

		[Given(@"I will create payment channel info")]
		public void GivenIWillCreatePaymentChannelInfo()
		{
			_paymentChannelCode = "TestChannelCode";
		}

		[Then(@"the payment channel created")]
		public async Task ThenThePaymentChannelCreatedAsync()
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			var channelData = await dbContext.PaymentChannels
				.SingleAsync(c => c.Code == _paymentChannelCode);

			Assert.IsNotNull(channelData);
		}

		[When(@"call create payment channel api")]
		public async Task WhenCallCreatePaymentChannelApiAsync()
		{
			await _paymentServiceClient.CreatePaymentChannelAsync(_paymentChannelCode);
		}
	}
}
