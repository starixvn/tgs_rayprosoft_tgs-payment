using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
	public class QueryWithdrawFormsSteps
	{
		private PagingResponse<WithdrawFormInfo> _actual;
		private PaymentServiceClient _paymentServiceClient;
		private IHost _testHost;

		public QueryWithdrawFormsSteps()
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

		[Given(@"I Query withdraw forms")]
		public void GivenIQueryWithdrawForms()
		{
		}

		[Then(@"return withdraw forms list")]
		public void ThenReturnWithdrawFormsList()
		{
			Assert.AreEqual(0, _actual.TotalCount);
			Assert.AreEqual(-1, _actual.Rows);
			Assert.NotNull(_actual.Items);
		}

		[When(@"call get withdraw forms api")]
		public async Task WhenCallGetWithdrawFormsApiAsync()
		{
			var conditions = new WithdrawFormQueryConditions
			{
				BrandId = Guid.NewGuid()
			};

			var paging = new Paging
			{
				ShowAll = true
			};

			_actual = await _paymentServiceClient.QueryWithdrawFormsAsync(conditions, paging);
		}
	}
}
