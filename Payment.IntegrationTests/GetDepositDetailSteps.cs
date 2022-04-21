using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

[Binding]
[Scope(Scenario = "Get deposit detail")]
public class GetDepositDetailSteps
{
	private DepositFormInfo _actual;
	private Guid _brandId;
	private Guid _formId;
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;

	public GetDepositDetailSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUpAsync() => _testHost.DisposeAsync();

	[Given(@"a form id \{(.*)}")]
	public void GivenAFormId(Guid formId)
	{
		_formId = formId;
	}

	[Given(@"I have a brand id \{(.*)}")]
	public void GivenIHaveABrandId(Guid brandId)
	{
		_brandId = brandId;
	}

	[Then(@"return detail data")]
	public void ThenReturnDetailData()
	{
		Assert.IsNotNull(_actual);
	}

	[When(@"call get deposit form detail api")]
	public async Task WhenCallGetDepositFormDetailApiAsync()
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
			BrandId = _brandId,
			PlayerCode = "PlayerCode",
			BrandPaymentId = bp.Id,
			Amount = 1000M,
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now
		});

		_ = await dbContext.SaveChangesAsync();

		_actual = await _paymentServiceClient.GetDepositFormAsync(_brandId, _formId);
	}
}
