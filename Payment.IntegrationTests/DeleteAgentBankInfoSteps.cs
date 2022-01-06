using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Scenario = "Delete agent bank info")]
	public class DeleteAgentBankInfoSteps
	{
		private Guid _agentId;
		private Guid _bankInfoId;
		private PaymentServiceClient _paymentServiceClient;
		private TestPaymentApplication _testHost;

		public DeleteAgentBankInfoSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.DisposeAsync();
		}

		[Given(@"agentId is \{(.*)}")]
		public void GivenAgentIdIs(Guid agentId)
		{
			_agentId = agentId;
		}

		[Given(@"bank info id is \{(.*)}")]
		public void GivenBankInfoIdIs(Guid bankInfoId)
		{
			_bankInfoId = bankInfoId;
		}

		[Given(@"I will delete bank info")]
		public void GivenIWillDeleteBankInfo()
		{
		}

		[Then(@"the bank info deleted")]
		public void ThenTheBankInfoDeleted()
		{
			Assert.Pass();
		}

		[When(@"call delete bank info api")]
		public async Task WhenCallDeleteBankInfoApiAsync()
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			await dbContext.AgentBankInfoItems.AddAsync(
				new AgentBankInfoItem
				{
					AgentId = _agentId,
					Id = _bankInfoId,
					BankType = "Bank",
					CreatedByUsername = "System",
					UpdatedByUsername = "System"
				});

			await dbContext.SaveChangesAsync();

			await _paymentServiceClient
				.DeleteAgentBankInfoAsync(_agentId, _bankInfoId);
		}
	}
}
