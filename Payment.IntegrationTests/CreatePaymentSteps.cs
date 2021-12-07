using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
    [Scope(Scenario = "Create a payment")]
    public class CreatePaymentSteps
    {
        private readonly PaymentServiceClient _paymentServiceClient;
        private readonly IHost _testHost;
        private PaymentCreateInfo _createInfo;

        public CreatePaymentSteps()
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

        [Given(@"I will create payment info")]
        public void GivenIWillCreatePaymentInfo()
        {
            _createInfo = new PaymentCreateInfo
            {
                Code = "Test"
            };
        }

        [Then(@"the payment info created")]
        public async Task ThenThePaymentInfoCreatedAsync()
        {
            var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

            var paymentData = await dbContext.PaymentItems
                .SingleAsync(p => p.Code == _createInfo.Code);

            Assert.IsNotNull(paymentData);
        }

        [When(@"call create payment api")]
        public async Task WhenCallCreatePaymentApiAsync()
        {
            await _paymentServiceClient.CreatePaymentAsync(_createInfo);
        }
    }
}