using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Get Untreated deposit form Count")]
public class GetUntreatedDepositCountSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private int _actual;
	private Guid _brandId;

	public GetUntreatedDepositCountSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I will count untreated deposit form count")]
	public void GivenIWillCountUntreatedDepositFormCount()
	{
		_brandId = Guid.NewGuid();
	}

	[Then(@"return form count")]
	public void ThenReturnFormCount()
	{
		ClassicAssert.AreEqual(2, _actual);
	}

	[When(@"call api")]
	public async Task WhenCallApiAsync()
	{
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

		await dbContext.DepositForms.AddRangeAsync(
			new[]
			{
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.WaitApprove,
						CreatedOn = DateTime.Now.AddMinutes(-5),
						UpdatedOn = DateTime.Now
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.Approved,
						CreatedOn = DateTime.Now.AddMinutes(-5),
						UpdatedOn = DateTime.Now
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.Init,
						CreatedOn = DateTime.Now.AddMinutes(-5),
						UpdatedOn = DateTime.Now
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.Reject,
						CreatedOn = DateTime.Now.AddMinutes(-5),
						UpdatedOn = DateTime.Now
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.WaitPayment,
						CreatedOn = DateTime.Now.AddMinutes(-5),
						UpdatedOn = DateTime.Now
					}
			});

		_ = await dbContext.SaveChangesAsync();

		_actual = await _paymentServiceClient.CountUntreatedDepositFormsAsync(_brandId);
	}
}
