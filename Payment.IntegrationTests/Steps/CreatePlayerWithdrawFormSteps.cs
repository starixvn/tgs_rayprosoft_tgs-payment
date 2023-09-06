using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TechTalk.SpecFlow;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Create player withdraw form")]
public class CreatePlayerWithdrawFormSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _bankInfoId;
	private Guid _createFormId;
	private string _playerCode;

	public CreatePlayerWithdrawFormSteps()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePlayerBucketServiceClient = Substitute.For<IPlayerBucketClient>();

		var walletId = Guid.NewGuid();
		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
			Arg.Any<string>(),
			Arg.Any<CancellationToken>())
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = walletId.ToString()
			}.ToImmutableDictionary());

		_ = fakeCashServiceClient.GetBalanceAsync(
			Arg.Is(walletId),
			Arg.Any<CancellationToken>())
			.Returns(10000M);
		_ = fakeCashServiceClient.LockWithdrawAsync(
			Arg.Is(walletId),
			Arg.Any<decimal>(),
			Arg.Any<string>(),
			Arg.Any<string>(),
			Arg.Any<CancellationToken>())
			.Returns(new TransactionResponse
			{
				Balance = 1000M,
				LockedBalance = 1000M,
				TransactionId = Guid.NewGuid()
			});

		_ = fakePlayerBucketServiceClient.GetBrandBucketTypesAsync(
			Arg.Any<Guid>(),
			Arg.Any<CancellationToken>())
			.Returns(AsyncEnumerable.Empty<string>());

		_ = fakePlayerBucketServiceClient.ReadBucketsAsync(
			Arg.Any<string>(),
			Arg.Any<CancellationToken>())
			.Returns(new ReadBucketResult(0M, null));

		_testHost = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakeCashServiceClient)
				.AddSingleton(fakePlayerServiceClient)
				.AddSingleton(fakePlayerBucketServiceClient));

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

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

		_ = await dbContext.PlayerBankInfos.AddAsync(
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

		_ = await dbContext.Brands.AddAsync(
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

		_ = await dbContext.SaveChangesAsync();

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
