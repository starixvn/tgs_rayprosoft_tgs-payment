using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
    public class QueryDepositFormsSteps
    {
        private readonly PaymentServiceClient _paymentServiceClient;
        private readonly IHost _testHost;
        private PagingResponse<Tgs.Payment.Clients.DepositFormInfo> _actual;

        public QueryDepositFormsSteps()
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

        [Given(@"I Query deposit forms")]
        public void GivenIQueryDepositForms()
        {
        }

        [Then(@"return deposit forms list")]
        public void ThenReturnDepositFormsList()
        {
            Assert.AreEqual(0, _actual.TotalCount);
            Assert.AreEqual(-1, _actual.Rows);
            Assert.NotNull(_actual.Items);
        }

        [When(@"call get deposit forms api")]
        public async Task WhenCallGetDepositFormsApiAsync()
        {
            var conditions = new DepositFormQueryConditions
            {
                BrandId = Guid.NewGuid()
            };
            var paging = new Paging
            {
                ShowAll = true
            };

            _actual = await _paymentServiceClient.QueryDepositFormsAsync(conditions, paging);
        }
    }
}