using NUnit.Framework;
using NUnit.Framework.Legacy;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests.Steps;

[Binding]
public class QueryDepositFormsSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private DepositForms? _actual;

	public QueryDepositFormsSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I Query deposit forms")]
	public void GivenIQueryDepositForms()
	{
	}

	[Then(@"return deposit forms list")]
	public void ThenReturnDepositFormsList()
	{
		ClassicAssert.AreEqual(0, _actual!.TotalCount);
		ClassicAssert.AreEqual(-1, _actual.Rows);
		ClassicAssert.NotNull(_actual.Items);
		ClassicAssert.AreEqual(0, _actual.Sum);
	}

	[When(@"call get deposit forms api")]
	public async Task WhenCallGetDepositFormsApiAsync()
	{
		var conditions = new DepositFormQueryConditions
		{
			BrandId = Guid.NewGuid()
		};
		var paging = new Paging
		{
			ShowAll = true
		};

		_actual = await _paymentServiceClient.QueryDepositFormsAsync(conditions, paging);
	}
}
