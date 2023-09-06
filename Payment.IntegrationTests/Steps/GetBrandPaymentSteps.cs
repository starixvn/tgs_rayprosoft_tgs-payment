using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
public class GetBrandPaymentSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private BrandPaymentInfo _actual;
	private Guid _brandPaymentId;
	private readonly TestPaymentApplication _testHost;

	public GetBrandPaymentSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I has a brand payment id \{(.*)}")]
	public async Task GivenIHasABrandPaymentIdAsync(Guid brandPaymentId)
	{
		_brandPaymentId = brandPaymentId;

		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var paymentInfo = new PaymentItem
		{
			Id = Guid.NewGuid(),
			Code = Guid.NewGuid().ToString(),
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now,
			CreatedByUsername = "System",
			UpdatedByUsername = "System"
		};

		_ = await dbContext.PaymentItems.AddAsync(paymentInfo);

		var paymentMethod = new PaymentMethod
		{
			Id = Guid.NewGuid(),
			Code = "TestMethod",
			BankType = "Bank",
		};

		_ = await dbContext.PaymentMethods.AddAsync(paymentMethod);

		var paymentChannel = new PaymentChannel
		{
			Id = Guid.NewGuid(),
			Code = "TestChannel"
		};

		_ = await dbContext.PaymentChannels.AddAsync(paymentChannel);

		var pmc = new PaymentMethodChannel
		{
			Id = Guid.NewGuid(),
			PaymentId = paymentInfo.Id,
			MethodId = paymentMethod.Id,
			ChannelId = paymentChannel.Id
		};

		_ = await dbContext.PaymentMethodChannels.AddAsync(pmc);

		var brandPayment = new BrandPayment
		{
			Id = _brandPaymentId,
			BrandId = Guid.NewGuid(),
			PaymentMethodChannelId = pmc.Id,
			ChannelName = "ChannelName",
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now,
			CreatedByUsername = "System",
			UpdatedByUsername = "System",
			ExtendedData = "{}"
		};

		_ = await dbContext.BrandPayments.AddAsync(brandPayment);

		_ = await dbContext.SaveChangesAsync();
	}

	[Then(@"return brand payment data")]
	public void ThenReturnBrandPaymentData()
	{
		Assert.NotNull(_actual);
	}

	[When(@"call get brand payment api")]
	public async Task WhenCallGetBrandPaymentApiAsync()
	{
		_actual = await _paymentServiceClient.GetBrandPaymentByIdAsync(_brandPaymentId);
	}
}
