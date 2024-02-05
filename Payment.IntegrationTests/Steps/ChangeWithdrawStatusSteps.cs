using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Change a withdraw form status")]
public class ChangeWithdrawStatusSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _formId;

	public ChangeWithdrawStatusSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I have a withdraw form")]
	public async Task GivenIHaveAWithdrawFormAsync()
	{
		_formId = Guid.NewGuid();

		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var brand = new Brand
		{
			Id = Guid.NewGuid(),
			Code = "GPI",
			ApiKey = "apikey",
			ClientId = "clientId",
			CreatedByUsername = "System",
			Name = "test",
			SecretKey = "secret",
			UpdatedByUsername = "System"
		};

		_ = await dbContext.Brands.AddAsync(brand);

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

		_ = await dbContext.WithdrawForms.AddAsync(new WithdrawForm
		{
			Id = _formId,
			BrandId = Guid.NewGuid(),
			UserId = "UserId",
			UserName = "UserName",
			UserType = (int)UserType.Player,
			BrandPaymentId = bp.Id,
			Amount = 1000M,
			Status = (int)PaymentFlowState.WaitApprove,
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now,
			BankInfo = "{}"
		});

		_ = await dbContext.SaveChangesAsync();
	}

	[Then(@"form status change to ""(.*)""")]
	public async Task ThenFormStatusChangeToAsync(PaymentFlowState expectedState)
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var formData = await dbContext.WithdrawForms.FindAsync(_formId);

		var formStatus = (PaymentFlowState)formData!.Status;

		ClassicAssert.AreEqual(expectedState, formStatus);
	}

	[When(@"I change status to ""(.*)""")]
	public async Task WhenIChangeStatusToAsync(PaymentFlowState state)
	{
		await _paymentServiceClient.SubmitWithdrawFormStatusAsync(
			_formId,
			state);
	}
}
