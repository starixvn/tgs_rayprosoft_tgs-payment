using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	[Binding]
    [Scope(Scenario = "List player bank infos")]
    public class GetPlayerBankInfosSteps
    {
        private PaymentServiceClient _paymentServiceClient;
        private string _playerCode;
        private IHost _testHost;

        public GetPlayerBankInfosSteps()
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

        [Given(@"I has a playerCode ""(.*)""")]
        public void GivenIHasAPlayerCode(string playerCode)
        {
            _playerCode = playerCode;
        }

        [Then(@"return bank info data by player")]
        public void ThenReturnBankInfoDataByPlayer()
        {
            Assert.Pass();
        }

        [When(@"call get bank infos api")]
        public async Task WhenCallGetBankInfosApiAsync()
        {
            var bankInfos = _paymentServiceClient.GetPlayerBankInfosAsync(_playerCode);

            await bankInfos.ToArrayAsync();
        }
    }
}