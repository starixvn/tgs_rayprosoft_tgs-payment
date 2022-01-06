using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	[Scope(Scenario = "Create player withdraw form")]
	public class CreatePlayerWithdrawFormSteps
	{
		private Guid _bankInfoId;
		private Guid _createFormId;
		private PaymentServiceClient _paymentServiceClient;
		private string _playerCode;
		private TestPaymentApplication _testHost;

		public CreatePlayerWithdrawFormSteps()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_paymentServiceClient = _testHost.GetPaymentServiceClient();
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.DisposeAsync();
		}

		[Given(@"I will create withdraw form")]
		public void GivenIWillCreateWithdrawForm()
		{
		}

		[Given(@"playerCode is ""(.*)""")]
		public void GivenPlayerCodeIs(string playerCode)
		{
			_playerCode = playerCode;
			_bankInfoId = Guid.NewGuid();
		}

		[Then(@"withdraw form created")]
		public void ThenWithdrawFormCreated()
		{
			Assert.AreNotEqual(Guid.Empty, _createFormId);
		}

		[When(@"call create withdraw form api")]
		public async Task WhenCallCreateWithdrawFormApiAsync()
		{
			var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

			var brandId = Guid.NewGuid();

			await dbContext.PlayerBankInfos.AddAsync(
				new Tgs.Payment.Repositories.Entities.PlayerBankInfo
				{
					Id = _bankInfoId,
					BankType = "Bank",
					BrandId = brandId,
					PlayerCode = _playerCode,
					Detail = "{}",
					CreatedOn = DateTime.Now,
					UpdatedOn = DateTime.Now,
					CreatedByUsername = "System",
					UpdatedByUsername = "System"
				});

			await dbContext.Brands.AddAsync(
				new Brand
				{
					Id = brandId,
					Code = "GPI",
					ApiKey = "apikey",
					ClientId = "clientId",
					CreatedByUsername = "System",
					Name = "test",
					SecretKey = "secret",
					UpdatedByUsername = "System"
				});

			await dbContext.SaveChangesAsync();

			var withdrawForm = new WithdrawFormCreateInfo
			{
				Amount = 1000M,
				BankInfoId = _bankInfoId,
				UserId = _playerCode,
				UserName = "UserName",
				BrandId = brandId,
				UserType = UserType.Player
			};

			_createFormId = await _paymentServiceClient.CreateWithdrawFormAsync(withdrawForm);
		}
	}
}
