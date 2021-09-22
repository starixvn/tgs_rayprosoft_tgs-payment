using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
    public class CreateDepositFormSteps
    {
        private readonly PaymentServiceClient _paymentServiceClient;
        private readonly IHost _testHost;
        private Guid _createdFormId;
        private DepositFormCreateInfo _depositForm;

        public CreateDepositFormSteps()
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

        [Given(@"I will create deposit form")]
        public void GivenIWillCreateDepositForm()
        {
            _depositForm = new DepositFormCreateInfo
            {
                Amount = 1000M,
                ApplyInfo = "{}",
                BrandId = Guid.NewGuid(),
                BrandPaymentId = Guid.NewGuid(),
                PlayerCode = "PlayerCode",
                PromotionId = null
            };
        }

        [Then(@"deposit form created")]
        public void ThenDepositFormCreated()
        {
            Assert.AreNotEqual(Guid.Empty, _createdFormId);
        }

        [When(@"call payment api")]
        public async Task WhenCallPaymentApiAsync()
        {
            _createdFormId = Guid.NewGuid();
            await _paymentServiceClient.CreateDepositFormAsync(_createdFormId, _depositForm);
        }
    }
}