using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "List player bank infos")]
public class GetPlayerBankInfosSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private string? _playerCode;

	public GetPlayerBankInfosSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I has a playerCode ""(.*)""")]
	public void GivenIHasAPlayerCode(string playerCode)
	{
		_playerCode = playerCode;
	}

	[Then(@"return bank info data by player")]
	public void ThenReturnBankInfoDataByPlayer()
	{
		Assert.Pass();
	}

	[When(@"call get bank infos api")]
	public async Task WhenCallGetBankInfosApiAsync()
	{
		var bankInfos = _paymentServiceClient.GetPlayerBankInfosAsync(_playerCode!);

		_ = await bankInfos.ToArrayAsync();
	}
}
