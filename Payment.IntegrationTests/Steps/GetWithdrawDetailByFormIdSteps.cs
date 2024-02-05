using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Get withdraw detail by form id")]
public class GetWithdrawDetailByFormIdSteps
{
	private object? _actual;
	private Guid _formId;
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;

	public GetWithdrawDetailByFormIdSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I have a form id \{(.*)}")]
	public void GivenIHaveAFormId(Guid formId)
	{
		_formId = formId;
	}

	[Then(@"return detail data")]
	public void ThenReturnDetailData()
	{
		ClassicAssert.IsNotNull(_actual);
	}

	[When(@"call get withdraw form detail api")]
	public async Task WhenCallGetWithdrawFormDetailApiAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

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
			UserId = "PlayerCode",
			UserName = "UserName",
			UserType = (int)UserType.Player,
			BrandPaymentId = bp.Id,
			Amount = 1000M,
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now,
			BankInfo = "{}"
		});

		_ = await dbContext.SaveChangesAsync();

		_actual = await _paymentServiceClient.GetWithdrawFormAsync(_formId);
	}
}
