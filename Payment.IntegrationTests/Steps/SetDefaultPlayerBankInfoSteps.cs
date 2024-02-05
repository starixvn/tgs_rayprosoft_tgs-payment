using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Tag = "SetDefaultBankInfo")]
public class SetDefaultPlayerBankInfoSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _bankInfoId;
	private string? _modifyUser;
	private string? _playerCode;

	public SetDefaultPlayerBankInfoSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[Given(@"bank info id is \{(.*)}")]
	public void GivenBankInfoIdIs(Guid bankInfoId)
	{
		_bankInfoId = bankInfoId;
	}

	[Given(@"I will set bank info to default")]
	public void GivenIWillSetBankInfoToDefault()
	{
	}

	[Given(@"modifyUser is ""([^""]*)""")]
	public void GivenModifyUserIs(string modifyUser)
	{
		_modifyUser = modifyUser;
	}

	[Given(@"playerCode is ""([^""]*)""")]
	public void GivenPlayerCodeIs(string playerCode)
	{
		_playerCode = playerCode;
	}

	[Then(@"the bank info is set to default")]
	public async Task ThenTheBankInfoIsSetToDefaultAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var data = await dbContext.PlayerBankInfos.FindAsync(_bankInfoId);

		ClassicAssert.AreEqual(_bankInfoId, data!.Id);
		ClassicAssert.IsTrue(data.Default);
	}

	[When(@"call set default bank info api")]
	public async Task WhenCallSetDefaultBankInfoApiAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.PlayerBankInfos.AddAsync(
			new Tgs.Payment.Repositories.Entities.PlayerBankInfo
			{
				PlayerCode = _playerCode!,
				Id = _bankInfoId,
				BankType = "Bank",
				Default = false,
				CreatedByUsername = "System",
				UpdatedByUsername = "System"
			});

		_ = await dbContext.SaveChangesAsync();

		await _paymentServiceClient
			.SetDefaultPlayerBankInfoAsync(_playerCode!, _bankInfoId, _modifyUser!);
	}
}
