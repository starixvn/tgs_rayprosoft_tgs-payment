using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests.Steps;

[Binding]
public class QueryWithdrawFormsSteps
{
	private WithdrawForms _actual;
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;

	public QueryWithdrawFormsSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUpAsync() => _testHost.DisposeAsync();

	[Given(@"I Query withdraw forms")]
	public void GivenIQueryWithdrawForms()
	{
	}

	[Then(@"return withdraw forms list")]
	public void ThenReturnWithdrawFormsList()
	{
		Assert.AreEqual(0, _actual.TotalCount);
		Assert.AreEqual(-1, _actual.Rows);
		Assert.NotNull(_actual.Items);
		Assert.AreEqual(0, _actual.Sum);
	}

	[When(@"call get withdraw forms api")]
	public async Task WhenCallGetWithdrawFormsApiAsync()
	{
		var conditions = new WithdrawFormQueryConditions
		{
			BrandId = Guid.NewGuid()
		};

		var paging = new Paging
		{
			ShowAll = true
		};

		_actual = await _paymentServiceClient.QueryWithdrawFormsAsync(conditions, paging);
	}
}
