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
	[Scope(Scenario = "Create agent withdraw form")]
	public class CreateAgentWithdrawFormSteps
	{
		private Guid _agentId;
		private Guid _bankInfoId;
		private Guid _createFormId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public CreateAgentWithdrawFormSteps()
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

		[Given(@"agentId is \{(.*)}")]
		public void GivenAgentIdIs(Guid agentId)
		{
			_agentId = agentId;
			_bankInfoId = Guid.NewGuid();
		}

		[Given(@"I will create withdraw form")]
		public void GivenIWillCreateWithdrawForm()
		{
		}

		[Then(@"withdraw form created")]
		public void ThenWithdrawFormCreated()
		{
			Assert.AreNotEqual(Guid.Empty, _createFormId);
		}

		[When(@"call create withdraw form api")]
		public async Task WhenCallCreateWithdrawFormApiAsync()
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			var brandId = Guid.NewGuid();

			await dbContext.AgentBankInfoItems.AddAsync(
				new AgentBankInfoItem
				{
					Id = _bankInfoId,
					BankType = "Bank",
					BrandId = brandId,
					AgentId = _agentId,
					Detail = "{}",
					CreatedOn = DateTime.Now,
					UpdatedOn = DateTime.Now
				});

			await dbContext.SaveChangesAsync();

			var withdrawForm = new WithdrawFormCreateInfo
			{
				Amount = 1000M,
				BankInfoId = _bankInfoId,
				UserId = _agentId.ToString(),
				UserName = "AgentName",
				BrandId = brandId,
				UserType = UserType.Agent
			};

			_createFormId = await _paymentServiceClient.CreateWithdrawFormAsync(withdrawForm);
		}
	}
}
