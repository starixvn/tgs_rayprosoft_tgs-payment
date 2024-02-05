using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Get brand payment acls")]
public class QueryBrandPaymentAclsSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private Guid _brandPaymentId1;
	private Guid _brandPaymentId2;
	private string? _playerCode;
	private readonly TestPaymentApplication _testHost;

	public QueryBrandPaymentAclsSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"brand payment id is \{(.*)}、\{(.*)}")]
	public void GivenBrandPaymentIdIs(Guid brandPaymentId1, Guid brandPaymentId2)
	{
		_brandPaymentId1 = brandPaymentId1;
		_brandPaymentId2 = brandPaymentId2;
	}

	[Given(@"I will get brand payment acls")]
	public void GivenIWillGetBrandPaymentAcls()
	{
	}

	[Given(@"playerCode is ""(.*)""")]
	public void GivenPlayerCodeIs(string playerCode)
	{
		_playerCode = playerCode;
	}

	[Then(@"return acl data")]
	public void ThenReturnAclData()
	{
		Assert.Pass();
	}

	[When(@"call get brand payment acl api")]
	public async Task WhenCallGetBrandPaymentAclApiAsync()
	{
		var acls = _paymentServiceClient.GetBrandPaymentAclsAsync(
			_playerCode!,
			new[] { _brandPaymentId1, _brandPaymentId2 });

		_ = await acls.ToArrayAsync();
	}
}
