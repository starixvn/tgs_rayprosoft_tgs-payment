using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests.Steps;

[Binding]
public class AllPaymentChannelsSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;

	public AllPaymentChannelsSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUpAsync() => _testHost.DisposeAsync();

	[Given(@"I will list all payment channels")]
	public void GivenIWillListAllPaymentChannels()
	{
	}

	[Then(@"return all payment channels data")]
	public void ThenReturnAllPaymentChannelsData()
	{
		Assert.Pass();
	}

	[When(@"call get all payment channels")]
	public async Task WhenCallGetAllPaymentChannelsAsync()
	{
		var paymentChannels = _paymentServiceClient.GetAllPaymentChannelsAsync();

		_ = await paymentChannels.ToArrayAsync();
	}
}
