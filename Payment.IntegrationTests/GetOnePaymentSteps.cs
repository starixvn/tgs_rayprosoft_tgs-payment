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
	public class GetOnePaymentSteps
	{
		private PaymentInfo _actual;
		private string _paymentCode;
		private Guid _paymentId;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public GetOnePaymentSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();
			_testHost.StartAsync();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public void CleanUp()
		{
			_testHost.StopAsync();
		}

		[Given(@"I has a payment id \{(.*)}")]
		public async Task GivenIHasAPaymentIdAsync(Guid paymentId)
		{
			_paymentId = paymentId;
			_paymentCode = Guid.NewGuid().ToString();

			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			var paymentInfo = new PaymentItem
			{
				Id = paymentId,
				Code = _paymentCode,
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now
			};

			await dbContext.PaymentItems.AddAsync(paymentInfo);

			await dbContext.SaveChangesAsync();
		}

		[Then(@"return payment data")]
		public void ThenReturnPaymentData()
		{
			Assert.AreEqual(_paymentCode, _actual.Code);
		}

		[When(@"call get payment api")]
		public async Task WhenCallGetPaymentApiAsync()
		{
			_actual = await _paymentServiceClient.GetPaymentByIdAsync(_paymentId);
		}
	}
}
