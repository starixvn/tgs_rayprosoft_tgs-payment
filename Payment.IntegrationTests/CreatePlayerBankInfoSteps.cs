using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

[Binding]
[Scope(Tag = "CreateBankInfo")]
public class CreatePlayerBankInfoSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private BankCreateInfo _bankInfo;
	private Guid _brandId;
	private Guid _createdId;
	private string _playerCode;

	public CreatePlayerBankInfoSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public async Task CleanUpAsync()
	{
		await _testHost.DisposeAsync();
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

	[Given(@"playerCode is ""(.*)""")]
	public void GivenPlayerCodeIs(string playerCode)
	{
		_playerCode = playerCode;
	}

	[Then(@"the bank info created")]
	public async Task ThenTheBankInfoCreatedAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var data = await dbContext.PlayerBankInfos.FindAsync(_createdId);

		Assert.AreEqual(_brandId, data.BrandId);
		Assert.AreEqual(_bankInfo.PaymentChannelId, data.PaymentChannelId);
	}

	[When(@"call create bank info api")]
	public async Task WhenCallCreateBankInfoApiAsync()
	{
		_createdId = await _paymentServiceClient
			.CreatePlayerBankInfoAsync(_brandId, _playerCode, _bankInfo);
	}
}
