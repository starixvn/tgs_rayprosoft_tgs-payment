using System;
using Microsoft.AspNetCore.TestHost;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Microsoft.Extensions.DependencyInjection;
using Tgs.Payment.Repositories.Entities;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Payment.IntegrationTests
{
	[Binding]
	public class GetOnePaymentSteps
	{
		private IHost _testHost;
		private PaymentServiceClient _paymentServiceClient;
		private Guid _paymentId;
		private string _paymentCode;
		private PaymentInfo _actual;

		public GetOnePaymentSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();
			_testHost.StartAsync();

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

		[When(@"call get payment api")]
		public async Task WhenCallGetPaymentApiAsync()
		{
			_actual = await _paymentServiceClient.GetPaymentByIdAsync(_paymentId);
		}

		[Then(@"return payment data")]
		public void ThenReturnPaymentData()
		{
			Assert.AreEqual(_paymentCode, _actual.Code);
		}
	}
}
