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
	[Scope(Scenario = "Get withdraw detail by form id")]
	public class GetWithdrawDetailByFormIdSteps
	{
		private IHost _testHost;
		private PaymentServiceClient _paymentServiceClient;
		private Guid _formId;
		private object _actual;

		public GetWithdrawDetailByFormIdSteps()
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

		[Given(@"I have a form id \{(.*)}")]
		public void GivenIHaveAFormId(Guid formId)
		{
			_formId = formId;
		}

		[When(@"call get withdraw form detail api")]
		public async Task WhenCallGetWithdrawFormDetailApiAsync()
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
				PaymentMethodChannelId = pmc.Id
			};
			await dbContext.BrandPayments.AddAsync(bp);

			await dbContext.WithdrawForms.AddAsync(new WithdrawForm
			{
				Id = _formId,
				BrandId = Guid.NewGuid(),
				UserId = "PlayerCode",
				UserName = "UserName",
				UserType = (int)UserType.Player,
				BrandPaymentId = bp.Id,
				Amount = 1000M,
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now
			});

			await dbContext.SaveChangesAsync();

			_actual = await _paymentServiceClient.GetWithdrawFormAsync(_formId);
		}

		[Then(@"return detail data")]
		public void ThenReturnDetailData()
		{
			Assert.IsNotNull(_actual);
		}
	}
}
