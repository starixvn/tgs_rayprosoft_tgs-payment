using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Scenario = "List agent bank infos")]
	public class GetAgentBankinfosSteps
	{
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;
		private Guid _agentId;

		public GetAgentBankinfosSteps()
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

		[Given(@"I has a agentId \{(.*)}")]
		public void GivenIHasAAgentId(Guid agentId)
		{
			_agentId = agentId;
		}

		[Then(@"return bank info data by angent")]
		public void ThenReturnBankInfoDataByAngent()
		{
			Assert.Pass();
		}

		[When(@"call get bank infos api")]
		public async Task WhenCallGetBankInfosApiAsync()
		{
			var bankInfos = _paymentServiceClient.GetAgentBankInfosAsync(_agentId);

			await bankInfos.ToArrayAsync();
		}
	}
}
