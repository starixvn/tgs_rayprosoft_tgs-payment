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
	public class GetBrandPaymentSteps
	{
		private BrandPaymentInfo _actual;
		private Guid _brandPaymentId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public GetBrandPaymentSteps()
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

		[Given(@"I has a brand payment id \{(.*)}")]
		public async Task GivenIHasABrandPaymentIdAsync(Guid brandPaymentId)
		{
			_brandPaymentId = brandPaymentId;

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

			await dbContext.SaveChangesAsync();
		}

		[Then(@"return brand payment data")]
		public void ThenReturnBrandPaymentData()
		{
			Assert.NotNull(_actual);
		}

		[When(@"call get brand payment api")]
		public async Task WhenCallGetBrandPaymentApiAsync()
		{
			_actual = await _paymentServiceClient.GetBrandPaymentByIdAsync(_brandPaymentId);
		}
	}
}
