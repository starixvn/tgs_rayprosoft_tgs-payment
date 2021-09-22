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
    [Scope(Scenario = "Review a deposit form")]
    public class ReviewDepositFormSteps
    {
        private Guid _formId;
        private PaymentServiceClient _paymentServiceClient;
        private IHost _testHost;

        public ReviewDepositFormSteps()
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

        [Given(@"I review a deposit form")]
        public async Task GivenIReviewADepositFormAsync()
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

        [Then(@"form status change to approved")]
        public async Task ThenFormStatusChangeToApprovedAsync()
        {
            var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

            var formData = await dbContext.DepositForms.FindAsync(_formId);

            var formStatus = (PaymentFlowState)formData.Status;

            Assert.AreEqual(PaymentFlowState.Approved, formStatus);
        }

        [When(@"approve form")]
        public async Task WhenApproveFormAsync()
        {
            await _paymentServiceClient.ReviewDepositFormAsync(_formId, ReviewState.Approve);
        }
    }
}