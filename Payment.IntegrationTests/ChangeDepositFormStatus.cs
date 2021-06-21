using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	public class ChangeDepositFormStatus
	{
		private Guid _formId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public ChangeDepositFormStatus()
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
		public void CleanUp()
		{
			_ = _testHost.StopAsync();
		}

		[Given(@"I have a deposit form")]
		public async Task GivenIHaveADepositFormAsync()
		{
			_formId = Guid.NewGuid();

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

			await dbContext.DepositForms.AddAsync(new DepositForm
			{
				Id = _formId,
				BrandId = Guid.NewGuid(),
				PlayerCode = "PlayerCode",
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

			var formData = await dbContext.DepositForms.FindAsync(_formId);

			var formStatus = (PaymentFlowState)formData.Status;

			Assert.AreEqual(expectedState, formStatus);
		}

		[When(@"I change status to ""(.*)""")]
		public async Task WhenIChangeStatusToAsync(PaymentFlowState state)
		{
			await _paymentServiceClient.SubmitDepositFormStatusAsync(_formId, state);
		}
	}
}
