using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Save thirdparty order no to deposit form")]
public class DepositSaveThirdPartyOrderNoSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _formId;
	private string _thirdPartyOrderNo;

	public DepositSaveThirdPartyOrderNoSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I get thirdparty order no ""(.*)""")]
	public void GivenIGetThirdpartyOrderNo(string thirdPartyOrderNo)
	{
		_formId = Guid.NewGuid();
		_thirdPartyOrderNo = thirdPartyOrderNo;
	}

	[Then(@"form thirdparty_order_no should be ""(.*)""")]
	public async Task ThenFormThirdparty_Order_NoShouldBeAsync(string thirdPartyOrderNo)
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var formData = await dbContext.DepositForms.FindAsync(_formId);

		Assert.AreEqual(thirdPartyOrderNo, formData.ThirdPartyOrderNo);
	}

	[When(@"I call save api")]
	public async Task WhenICallSaveApiAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.Players.AddAsync(new Player
		{
			PlayerCode = "PlayerCode",
			DefaultWalletCurrencyCode = "CNY",
			Password = Array.Empty<byte>(),
			Salt = Array.Empty<byte>(),
			Username = "player",
			WithdrawPassword = Array.Empty<byte>(),
			WithdrawSalt = Array.Empty<byte>()
		});

		var pmc = new PaymentMethodChannel
		{
			Id = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			MethodId = Guid.NewGuid(),
			ChannelId = Guid.NewGuid()
		};

		_ = await dbContext.PaymentMethodChannels.AddAsync(pmc);

		var bp = new BrandPayment
		{
			Id = Guid.NewGuid(),
			PaymentMethodChannelId = pmc.Id,
			ChannelName = "channel",
			CreatedByUsername = "System",
			ExtendedData = "{}",
			UpdatedByUsername = "System"
		};
		_ = await dbContext.BrandPayments.AddAsync(bp);

		_ = await dbContext.DepositForms.AddAsync(new DepositForm
		{
			Id = _formId,
			BrandId = Guid.NewGuid(),
			PlayerCode = "PlayerCode",
			BrandPaymentId = bp.Id,
			Amount = 1000M,
			Status = (int)PaymentFlowState.WaitApprove,
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now
		});

		_ = await dbContext.SaveChangesAsync();

		var form = await _paymentServiceClient.GetDepositFormAsync(_formId);

		await _paymentServiceClient.SubmitDepositFormStatusAsync(_formId, form.Status, thirdPartyOrderNo: _thirdPartyOrderNo);
	}
}
