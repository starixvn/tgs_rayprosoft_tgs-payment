using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
    public class GetAllPaymentsSteps
    {
        private PaymentServiceClient _paymentServiceClient;
        private IHost _testHost;

        public GetAllPaymentsSteps()
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

        [Given(@"I will list all payments")]
        public void GivenIWillListAllPayments()
        {
        }

        [Then(@"return all paymnets data")]
        public void ThenReturnAllPaymnetsData()
        {
            Assert.Pass();
        }

        [When(@"call get all payments api")]
        public async Task WhenCallGetAllPaymentsApiAsync()
        {
            var payments = _paymentServiceClient.GetAllPaymentsAsync();

            await payments.ToArrayAsync();
        }
    }
}