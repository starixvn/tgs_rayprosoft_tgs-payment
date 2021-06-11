using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
	public class DepositSteps
	{
		private readonly IHost _testHost;
		private readonly PaymentServiceClient _paymentServiceClient;
		private DepositFormCreateInfo _depositForm;

		public DepositSteps()
		{
			_testHost = Host.CreateDefaultBuilder(Array.Empty<string>())
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
						.UseStartup<PaymentService.Startup>()
						.UseTestServer();
				})
				.Build();

			_ = _testHost.StartAsync();

			_paymentServiceClient = new PaymentServiceClient(_testHost.GetTestClient(), null);
		}
		[Given(@"I will create deposit form")]
		public void GivenIWillCreateDepositForm()
		{
			_depositForm = new DepositFormCreateInfo
			{
				Amount = 1000M,
				ApplyInfo = "{}",
				BrandId = Guid.NewGuid(),
				BrandPaymentId = Guid.NewGuid(),
				PlayerCode = "PlayerCode",
				PromotionId = null
			};
		}

		[When(@"call payment api")]
		public async Task WhenCallPaymentApiAsync()
		{
			await _paymentServiceClient.CreateDepositFormAsync(_depositForm);
		}

		[Then(@"deposit form created")]
		public void ThenDepositFormCreated()
		{
			ScenarioContext.Current.Pending();
		}
	}
}
