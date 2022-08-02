using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Create agent bank info")]
public class CreateAgentBankinfoSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _agentId;
	private string _agentName;
	private BankCreateInfo _bankInfo;
	private Guid _brandId;
	private Guid _createdId;

	public CreateAgentBankinfoSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUpAsync() => _testHost.DisposeAsync();

	[Given(@"agentId is \{(.*)}")]
	public void GivenAgentIdIs(Guid agentId)
	{
		_agentId = agentId;
	}

	[Given(@"agentName is ""(.*)""")]
	public void GivenAgentNameIs(string agentName)
	{
		_agentName = agentName;
	}

	[Given(@"brandId is \{(.*)}")]
	public void GivenBrandIdIs(Guid brandId)
	{
		_brandId = brandId;
	}

	[Given(@"I will create bank info")]
	public void GivenIWillCreateBankInfo()
	{
		_bankInfo = new BankCreateInfo
		{
			BankType = "Bank",
			PaymentChannelId = Guid.NewGuid(),
			Detail = JsonDocument.Parse(JsonSerializer.Serialize(new
			{
				bankAccount = "123456890"
			}))
		};
	}

	[Then(@"the bank info created")]
	public async Task ThenTheBankInfoCreatedAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var data = await dbContext.AgentBankInfoItems.FindAsync(_createdId);

		Assert.AreEqual(_brandId, data.BrandId);
		Assert.AreEqual(_bankInfo.PaymentChannelId, data.PaymentChannelId);
	}

	[When(@"call create bank info api")]
	public async Task WhenCallCreateBankInfoApiAsync()
	{
		_createdId = await _paymentServiceClient
			.CreateAgentBankInfoAsync(_brandId, _agentId, _agentName, _bankInfo);
	}
}
