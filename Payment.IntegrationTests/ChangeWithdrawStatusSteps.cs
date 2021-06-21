using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Feature = "withdraw")]
	public class ChangeWithdrawStatusSteps
	{
		private Guid _formId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public ChangeWithdrawStatusSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_ = _testHost.StartAsync();

			_paymentServiceClient = new PaymentServiceClient(
				_testHost.GetTestClient(),
				new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.StopAsync();
		}

		[Given(@"I have a withdraw form")]
		public async Task GivenIHaveAWithdrawFormAsync()
		{
			_formId = Guid.NewGuid();

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
				PaymentMethodChannelId = pmc.Id
			};
			await dbContext.BrandPayments.AddAsync(bp);

			await dbContext.WithdrawForms.AddAsync(new WithdrawForm
			{
				Id = _formId,
				BrandId = Guid.NewGuid(),
				UserId = "UserId",
				UserName = "UserName",
				UserType = (int)UserType.Player,
				BrandPaymentId = bp.Id,
				Amount = 1000M,
				Status = (int)PaymentFlowState.WaitApprove,
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now
			});

			await dbContext.SaveChangesAsync();
		}

		[Then(@"form status change to ""(.*)""")]
		public async Task ThenFormStatusChangeToAsync(PaymentFlowState expectedState)
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			var formData = await dbContext.WithdrawForms.FindAsync(_formId);

			var formStatus = (PaymentFlowState)formData.Status;

			Assert.AreEqual(expectedState, formStatus);
		}

		[When(@"I change status to ""(.*)""")]
		public async Task WhenIChangeStatusToAsync(PaymentFlowState state)
		{
			await _paymentServiceClient.SubmitWithdrawFormStatusAsync(
				_formId,
				state);
		}
	}
}
