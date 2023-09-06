using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Change a deposit form status")]
public class ChangeDepositFormStatus
{
	private readonly TestPaymentApplication _testHost;
	private Guid _formId;
	private readonly PaymentServiceClient _paymentServiceClient;

	public ChangeDepositFormStatus()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I have a deposit form")]
	public async Task GivenIHaveADepositFormAsync()
	{
		_formId = Guid.NewGuid();

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
			ExtendedData = "{}",
			CreatedByUsername = "System",
			UpdatedByUsername = "System",
			ChannelName = "TestChannel"
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
	}

	[Then(@"form status change to ""(.*)""")]
	public async Task ThenFormStatusChangeToAsync(PaymentFlowState expectedState)
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var formData = await dbContext.DepositForms.FindAsync(_formId);

		var formStatus = (PaymentFlowState)formData.Status;

		Assert.AreEqual(expectedState, formStatus);
	}

	[When(@"I change status to ""(.*)""")]
	public async Task WhenIChangeStatusToAsync(PaymentFlowState state)
	{
		await _paymentServiceClient.SubmitDepositFormStatusAsync(_formId, state);
	}
}
