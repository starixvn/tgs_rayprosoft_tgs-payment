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
	[Scope(Scenario = "Get Untreated deposit form Count")]
	public class GetUntreatedDepositCountSteps
	{
		private int _actual;
		private Guid _brandId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public GetUntreatedDepositCountSteps()
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

		[Given(@"I will count untreated deposit form count")]
		public void GivenIWillCountUntreatedDepositFormCount()
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

			await dbContext.Players.AddAsync(new Player
			{
				PlayerCode = "PlayerCode"
			});

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
				PaymentMethodChannelId = pmc.Id
			};
			await dbContext.BrandPayments.AddAsync(bp);

			await dbContext.DepositForms.AddRangeAsync(
				new[]
				{
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.WaitApprove
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.Approved
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.Init
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.Reject
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.WaitPayment
					}
				});

			await dbContext.SaveChangesAsync();

			_actual = await _paymentServiceClient.CountUntreatedDepositFormsAsync(_brandId);
		}
	}
}
