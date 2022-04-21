using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests;

[Binding]
[Scope(Scenario = "List agent bank infos")]
public class GetAgentBankinfosSteps
{
	private Guid _agentId;
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;

	public GetAgentBankinfosSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUpAsync() => _testHost.DisposeAsync();

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

		_ = await bankInfos.ToArrayAsync();
	}
}
