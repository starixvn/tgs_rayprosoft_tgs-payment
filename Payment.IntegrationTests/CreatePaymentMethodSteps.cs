using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

[Binding]
[Scope(Scenario = "Create a payment method")]
public class CreatePaymentMethodSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private PaymentMethodCreateInfo _createInfo;

	public CreatePaymentMethodSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUpAsync() => _testHost.DisposeAsync();

	[Given(@"I will create payment method info")]
	public void GivenIWillCreatePaymentMethodInfo()
	{
		_createInfo = new PaymentMethodCreateInfo
		{
			Code = "MethodCode",
			BankType = "Bank"
		};
	}

	[Then(@"the payment channel created")]
	public async Task ThenThePaymentChannelCreatedAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var methodData = await dbContext.PaymentMethods
			.SingleAsync(m => m.Code == _createInfo.Code);

		Assert.IsNotNull(methodData);
		Assert.AreEqual(_createInfo.BankType, methodData.BankType);
	}

	[When(@"call create payment method api")]
	public async Task WhenCallCreatePaymentMethodApiAsync()
	{
		await _paymentServiceClient.CreatePaymentMethodAsync(_createInfo);
	}
}
