using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Review a deposit form")]
public class ReviewDepositFormSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _formId;

	public ReviewDepositFormSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I review a deposit form")]
	public async Task GivenIReviewADepositFormAsync()
	{
		_formId = Guid.NewGuid();

		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.Players.AddAsync(new Player
		{
			PlayerCode = "PlayerCode",
			DefaultWalletCurrencyCode = "CNY",
			Password = [],
			Salt = [],
			Username = "player",
			WithdrawPassword = [],
			WithdrawSalt = []
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
	}

	[Then(@"form status change to approved")]
	public async Task ThenFormStatusChangeToApprovedAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var formData = await dbContext.DepositForms.FindAsync(_formId);

		var formStatus = (PaymentFlowState)formData.Status;

		ClassicAssert.AreEqual(PaymentFlowState.Approved, formStatus);
	}

	[When(@"approve form")]
	public async Task WhenApproveFormAsync()
	{
		await _paymentServiceClient.ReviewDepositFormAsync(_formId, ReviewState.Approve, "93a723b2-9ebb-4607-ee8d-08d978fba75f", "GPI");
	}
}
