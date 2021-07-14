using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using PaymentFlowState = Tgs.Payment.PaymentFlowState;
using UserType = Tgs.Payment.UserType;

namespace Payment.IntegrationTests
{
	[Binding]
	public class ReviewWithdrawFormSteps
	{
		private Guid _brandPaymentId;
		private Guid _formId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public ReviewWithdrawFormSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost(
				services =>
				{
					var fakeWithdrawProcessorFactory = Substitute.For<IWithdrawProcessorFactory>();

					var fakeWithdrawProcessor = Substitute.For<IWithdrawProcessor>();
					fakeWithdrawProcessorFactory.GetWithdrawProcessor(Arg.Any<string>())
						.Returns(fakeWithdrawProcessor);

					services.AddSingleton(fakeWithdrawProcessorFactory);
				});
			_ = _testHost.StartAsync();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.StopAsync();
		}

		[Given(@"I review a withdraw form")]
		public async Task GivenIReviewAWithdrawFormAsync()
		{
			_formId = Guid.NewGuid();

			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			var paymentInfo = new PaymentItem
			{
				Id = Guid.NewGuid(),
				Code = Guid.NewGuid().ToString(),
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now
			};

			await dbContext.PaymentItems.AddAsync(paymentInfo);

			var paymentMethod = new PaymentMethod
			{
				Id = Guid.NewGuid(),
				Code = "TestMethod",
				BankType = "Bank"
			};

			await dbContext.PaymentMethods.AddAsync(paymentMethod);

			var paymentChannel = new PaymentChannel
			{
				Id = Guid.NewGuid(),
				Code = "TestChannel"
			};

			await dbContext.PaymentChannels.AddAsync(paymentChannel);

			var pmc = new PaymentMethodChannel
			{
				Id = Guid.NewGuid(),
				PaymentId = paymentInfo.Id,
				MethodId = paymentMethod.Id,
				ChannelId = paymentChannel.Id
			};

			await dbContext.PaymentMethodChannels.AddAsync(pmc);

			var brandPayment = new BrandPayment
			{
				Id = _brandPaymentId,
				BrandId = Guid.NewGuid(),
				PaymentMethodChannelId = pmc.Id,
				ChannelName = "ChannelName",
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now
			};

			await dbContext.BrandPayments.AddAsync(brandPayment);

			await dbContext.WithdrawForms.AddAsync(new WithdrawForm
			{
				Id = _formId,
				BrandId = Guid.NewGuid(),
				UserId = "PlayerCode",
				UserName = "Player",
				UserType = (int)UserType.Player,
				Amount = 1000M,
				Status = (int)PaymentFlowState.WaitApprove,
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now
			});

			await dbContext.SaveChangesAsync();

			_brandPaymentId = brandPayment.Id;
		}

		[Then(@"form status change to ""(.*)""")]
		public async Task ThenFormStatusChangeToAsync(PaymentFlowState state)
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			var formData = await dbContext.WithdrawForms.FindAsync(_formId);

			var formStatus = (PaymentFlowState)formData.Status;

			Assert.AreEqual(state, formStatus);
			Assert.AreEqual(_brandPaymentId, formData.BrandPaymentId);
		}

		[When(@"approve form")]
		public async Task WhenApproveFormAsync()
		{
			await _paymentServiceClient
				.ReviewWithdrawFormAsync(
					_formId,
					_brandPaymentId,
					Tgs.Payment.Clients.ReviewState.Approve);
		}
	}
}
