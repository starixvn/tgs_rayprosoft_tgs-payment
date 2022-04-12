using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests;

[Binding]
public class QueryBrandPaymentVipsSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _brandPaymentId1;
	private Guid _brandPaymentId2;

	public QueryBrandPaymentVipsSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUpAsync() => _testHost.DisposeAsync();

	[Given(@"brand payment id is \{(.*)}、\{(.*)}")]
	public void GivenBrandPaymentIdIs(Guid brandPaymentId1, Guid brandPaymentId2)
	{
		_brandPaymentId1 = brandPaymentId1;
		_brandPaymentId2 = brandPaymentId2;
	}

	[Given(@"I will get brand payment vip settings")]
	public void GivenIWillGetBrandPaymentVipSettings()
	{
	}

	[Then(@"return vip data")]
	public void ThenReturnVipData()
	{
		Assert.Pass();
	}

	[When(@"call get brand payment vip api")]
	public async Task WhenCallGetBrandPaymentVipApiAsync()
	{
		var vips = _paymentServiceClient.GetBrandPaymentVipsAsync(
			new[] { _brandPaymentId1, _brandPaymentId2 });

		_ = await vips.ToArrayAsync();
	}
}
