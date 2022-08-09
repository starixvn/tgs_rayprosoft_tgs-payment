using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests.Steps;

[Binding]
public class GetAllPaymentsSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;

	public GetAllPaymentsSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUpAsync() => _testHost.DisposeAsync();

	[Given(@"I will list all payments")]
	public void GivenIWillListAllPayments()
	{
	}

	[Then(@"return all paymnets data")]
	public void ThenReturnAllPaymnetsData()
	{
		Assert.Pass();
	}

	[When(@"call get all payments api")]
	public async Task WhenCallGetAllPaymentsApiAsync()
	{
		var payments = _paymentServiceClient.GetAllPaymentsAsync();

		_ = await payments.ToArrayAsync();
	}
}
