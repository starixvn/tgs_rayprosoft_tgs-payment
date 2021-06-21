using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Tag = "DeleteBankInfo")]
	public class DeletePlayerBankInfoSteps
	{
		private Guid _bankInfoId;
		private PaymentServiceClient _paymentServiceClient;
		private string _playerCode;
		private IHost _testHost;

		public DeletePlayerBankInfoSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_ = _testHost.StartAsync();

			_paymentServiceClient = new PaymentServiceClient(
				_testHost.GetTestClient(),
				new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.StopAsync();
		}

		[Given(@"bank info id is \{(.*)}")]
		public void GivenBankInfoIdIs(Guid bankInfoId)
		{
			_bankInfoId = bankInfoId;
		}

		[Given(@"I will delete bank info")]
		public void GivenIWillDeleteBankInfo()
		{
		}

		[Given(@"playerCode is ""(.*)""")]
		public void GivenPlayerCodeIs(string playerCode)
		{
			_playerCode = playerCode;
		}

		[Then(@"the bank info deleted")]
		public void ThenTheBankInfoDeleted()
		{
			Assert.Pass();
		}

		[When(@"call delete bank info api")]
		public async Task WhenCallDeleteBankInfoApiAsync()
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			await dbContext.PlayerBankInfos.AddAsync(
				new Tgs.Payment.Repositories.Entities.PlayerBankInfo
				{
					PlayerCode = _playerCode,
					Id = _bankInfoId
				});

			await dbContext.SaveChangesAsync();

			await _paymentServiceClient
				.DeletePlayerBankInfoAsync(_playerCode, _bankInfoId);
		}
	}
}
