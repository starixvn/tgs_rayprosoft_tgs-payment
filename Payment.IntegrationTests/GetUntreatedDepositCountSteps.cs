using Microsoft.Extensions.DependencyInjection;
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
		private TestPaymentApplication _testHost;

		public GetUntreatedDepositCountSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.DisposeAsync();
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
				PlayerCode = "PlayerCode",
				DefaultWalletCurrencyCode = "CNY",
				Password = Array.Empty<byte>(),
				Salt = Array.Empty<byte>(),
				Username = "player",
				WithdrawPassword = Array.Empty<byte>(),
				WithdrawSalt = Array.Empty<byte>()
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
				PaymentMethodChannelId = pmc.Id,
				ChannelName = "channel",
				CreatedByUsername = "System",
				ExtendedData = "{}",
				UpdatedByUsername = "System"
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
						Status = (int)PaymentFlowState.WaitApprove,
						UpdatedOn = DateTime.Now
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.Approved,
						UpdatedOn = DateTime.Now
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.Init,
						UpdatedOn = DateTime.Now
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.Reject,
						UpdatedOn = DateTime.Now
					},
					new DepositForm
					{
						Id = Guid.NewGuid(),
						BrandId = _brandId,
						PlayerCode = "PlayerCode",
						BrandPaymentId = bp.Id,
						Status = (int)PaymentFlowState.WaitPayment,
						UpdatedOn = DateTime.Now
					}
				});

			await dbContext.SaveChangesAsync();

			_actual = await _paymentServiceClient.CountUntreatedDepositFormsAsync(_brandId);
		}
	}
}
