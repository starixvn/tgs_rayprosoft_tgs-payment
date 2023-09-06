using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Get Untreated withdraw form Count")]
public class GetUntreatedWithdrawFormsSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private int _actual;
	private Guid _brandId;

	public GetUntreatedWithdrawFormsSteps()
	{
		_testHost = TestSiteHelper.GetPaymentServiceTestHost();

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I will count untreated withdraw form count")]
	public void GivenIWillCountUntreatedWithdrawFormCount()
	{
		_brandId = Guid.NewGuid();
	}

	[Then(@"return form count")]
	public void ThenReturnFormCount()
	{
		Assert.AreEqual(2, _actual);
	}

	[When(@"call api")]
	public async Task WhenCallApiAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var pmc = new PaymentMethodChannel
		{
			Id = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			MethodId = Guid.NewGuid(),
			ChannelId = Guid.NewGuid()
		};

		_ = await dbContext.PaymentMethodChannels.AddAsync(pmc);

		var bp = new BrandPayment
		{
			Id = Guid.NewGuid(),
			PaymentMethodChannelId = pmc.Id,
			ChannelName = "channel",
			CreatedByUsername = "System",
			ExtendedData = "{}",
			UpdatedByUsername = "System"
		};
		_ = await dbContext.BrandPayments.AddAsync(bp);

		await dbContext.WithdrawForms.AddRangeAsync(new[] {
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.Init,
					CreatedOn = DateTime.Now.AddMinutes(-15),
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				},
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.WaitApprove,
					CreatedOn = DateTime.Now.AddMinutes(-15),
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				},
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.Approved,
					CreatedOn = DateTime.Now.AddMinutes(-15),
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				},
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.Reject,
					CreatedOn = DateTime.Now.AddMinutes(-15),
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				},
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.Paid,
					CreatedOn = DateTime.Now.AddMinutes(-15),
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				}
			});

		_ = await dbContext.SaveChangesAsync();

		_actual = await _paymentServiceClient.CountUntreatedWithdrawFormsAsync(_brandId);
	}
}
