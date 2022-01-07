using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

[Binding]
[Scope(Scenario = "Create a payment channel")]
public class CreatePaymentChannelSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private string _paymentChannelCode;

	public CreatePaymentChannelSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public async Task CleanUpAsync()
	{
		await _testHost.DisposeAsync();
	}

	[Given(@"I will create payment channel info")]
	public void GivenIWillCreatePaymentChannelInfo()
	{
		_paymentChannelCode = "TestChannelCode";
	}

	[Then(@"the payment channel created")]
	public async Task ThenThePaymentChannelCreatedAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var channelData = await dbContext.PaymentChannels
			.SingleAsync(c => c.Code == _paymentChannelCode);

		Assert.IsNotNull(channelData);
	}

	[When(@"call create payment channel api")]
	public async Task WhenCallCreatePaymentChannelApiAsync()
	{
		await _paymentServiceClient.CreatePaymentChannelAsync(_paymentChannelCode);
	}
}
