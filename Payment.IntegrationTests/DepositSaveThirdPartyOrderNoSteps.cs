using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Scenario = "Save thirdparty order no to deposit form")]
	public class DepositSaveThirdPartyOrderNoSteps
	{
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;
		private Guid _formId;
		private string _thirdPartyOrderNo;

		public DepositSaveThirdPartyOrderNoSteps()
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

		[Given(@"I get thirdparty order no ""(.*)""")]
		public void GivenIGetThirdpartyOrderNo(string thirdPartyOrderNo)
		{
			_formId = Guid.NewGuid();
			_thirdPartyOrderNo = thirdPartyOrderNo;
		}

		[When(@"I call save api")]
		public async Task WhenICallSaveApi()
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			await dbContext.DepositForms.AddAsync(
				new DepositForm
				{
					Id = _formId
				});

			await dbContext.SaveChangesAsync();

			await _paymentServiceClient.SubmitDepositFormThirdPartyOrderNoAsync(_formId, _thirdPartyOrderNo);
		}

		[Then(@"form thirdparty_order_no should be ""(.*)""")]
		public async Task ThenFormThirdparty_Order_NoShouldBe(string thirdPartyOrderNo)
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			var formData = await dbContext.DepositForms.FindAsync(_formId);

			Assert.AreEqual(thirdPartyOrderNo, formData.ThirdPartyOrderNo);
		}
	}
}