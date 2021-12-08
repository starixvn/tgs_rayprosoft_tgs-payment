using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Scenario = "Get Untreated withdraw form Count")]
	public class GetUntreatedWithdrawFormsSteps
	{
		private int _actual;
		private Guid _brandId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public GetUntreatedWithdrawFormsSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();
			_ = _testHost.StartAsync();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.StopAsync();
		}

		[Given(@"I will count untreated withdraw form count")]
		public void GivenIWillCountUntreatedWithdrawFormCount()
		{
			_brandId = Guid.NewGuid();
		}

		[Then(@"return form count")]
		public void ThenReturnFormCount()
		{
			Assert.AreEqual(3, _actual);
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

			await dbContext.PaymentMethodChannels.AddAsync(pmc);

			var bp = new BrandPayment
			{
				Id = Guid.NewGuid(),
				PaymentMethodChannelId = pmc.Id,
				ChannelName = "channel",
				CreatedByUsername = "System",
				ExtendedData = "{}",
				UpdatedByUsername = "System"
			};
			await dbContext.BrandPayments.AddAsync(bp);

			await dbContext.WithdrawForms.AddRangeAsync(new[] {
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.Init,
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				},
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.WaitApprove,
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				},
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.Approved,
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				},
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.Reject,
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				},
				new WithdrawForm{
					Id = Guid.NewGuid(),
					BrandId = _brandId,
					Status = (int)PaymentFlowState.Paid,
					UpdatedOn = DateTime.Now,
					BankInfo = "{}"
				}
			});

			await dbContext.SaveChangesAsync();

			_actual = await _paymentServiceClient.CountUntreatedWithdrawFormsAsync(_brandId);
		}
	}
}
