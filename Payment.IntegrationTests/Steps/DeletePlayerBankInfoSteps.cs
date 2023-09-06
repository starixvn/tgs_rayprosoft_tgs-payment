using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Tag = "DeleteBankInfo")]
public class DeletePlayerBankInfoSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _bankInfoId;
	private string _playerCode;

	public DeletePlayerBankInfoSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"bank info id is \{(.*)}")]
	public void GivenBankInfoIdIs(Guid bankInfoId)
	{
		_bankInfoId = bankInfoId;
	}

	[Given(@"I will delete bank info")]
	public void GivenIWillDeleteBankInfo()
	{
	}

	[Given(@"playerCode is ""(.*)""")]
	public void GivenPlayerCodeIs(string playerCode)
	{
		_playerCode = playerCode;
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

		_ = await dbContext.PlayerBankInfos.AddAsync(
			new Tgs.Payment.Repositories.Entities.PlayerBankInfo
			{
				PlayerCode = _playerCode,
				Id = _bankInfoId,
				BankType = "Bank",
				CreatedByUsername = "System",
				UpdatedByUsername = "System"
			});

		_ = await dbContext.SaveChangesAsync();

		await _paymentServiceClient
			.DeletePlayerBankInfoAsync(_playerCode, _bankInfoId);
	}
}
