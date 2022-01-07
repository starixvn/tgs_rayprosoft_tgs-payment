using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests;

[Binding]
public class AllPaymentMethodsSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;

	public AllPaymentMethodsSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public async Task CleanUpAsync()
	{
		await _testHost.DisposeAsync();
	}

	[Given(@"I will list all payment methods")]
	public void GivenIWillListAllPaymentMethods()
	{
	}

	[Then(@"return all payment methods data")]
	public void ThenReturnAllPaymentMethodsData()
	{
		Assert.Pass();
	}

	[When(@"call get all payment methods api")]
	public async Task WhenCallGetAllPaymentMethodsApiAsync()
	{
		var paymentMethods = _paymentServiceClient.GetAllPaymentMethodsAsync();

		_ = await paymentMethods.ToArrayAsync();
	}
}
