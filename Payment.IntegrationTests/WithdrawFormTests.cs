using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using Tgs.PlayerService.Client;

namespace Payment.IntegrationTests;

public class WithdrawFormTests
{
	[Test]
	public async Task 玩家提款時會檢查錢包餘額是否足夠()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePlayerServiceClient)
				.AddSingleton(fakeCashServiceClient)
				.AddHttpClientLoggingHandler());

		var httpClient = web.CreateDefaultClient(web.Services.ResolveLoggingHandler<PaymentServiceClient>());

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var bankInfoId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.PlayerBankInfos.AddAsync(
			new Tgs.Payment.Repositories.Entities.PlayerBankInfo
			{
				Id = bankInfoId,
				BankType = "Bank",
				BrandId = brandId,
				PlayerCode = playerCode,
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

		var withdrawForm = new Tgs.Payment.Clients.WithdrawFormCreateInfo
		{
			Amount = 1000M,
			BankInfoId = bankInfoId,
			UserId = playerCode,
			UserName = "UserName",
			BrandId = brandId,
			UserType = Tgs.Payment.Clients.UserType.Player
		};

		var walletId = Guid.NewGuid();
		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = walletId.ToString()
			}.ToImmutableDictionary());

		_ = fakeCashServiceClient.GetBalanceAsync(
			Arg.Is(walletId),
			Arg.Any<CancellationToken>())
			.Returns(0M);

		_ = Assert.ThrowsAsync<Tgs.Payment.Clients.InsufficientFundsException>(
			async () => await sut.CreateWithdrawFormAsync(withdrawForm));

		_ = fakeCashServiceClient.Received(1)
			.GetBalanceAsync(
				Arg.Is(walletId),
				Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task 建立WithdrawForm會發出Event()
	{
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePaymentClient)
				.AddSingleton(fakePlayerServiceClient)
				.AddSingleton(fakeCashServiceClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var bankInfoId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.PlayerBankInfos.AddAsync(
			new Tgs.Payment.Repositories.Entities.PlayerBankInfo
			{
				Id = bankInfoId,
				BankType = "Bank",
				BrandId = brandId,
				PlayerCode = playerCode,
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

		var withdrawForm = new Tgs.Payment.Clients.WithdrawFormCreateInfo
		{
			Amount = 1000M,
			BankInfoId = bankInfoId,
			UserId = playerCode,
			UserName = "UserName",
			BrandId = brandId,
			UserType = Tgs.Payment.Clients.UserType.Player
		};

		var walletId = Guid.NewGuid();
		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
			Arg.Is(playerCode),
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
			Arg.Is(withdrawForm.Amount),
			Arg.Any<string>(),
			Arg.Any<string>(),
			Arg.Any<CancellationToken>())
			.Returns(new TransactionResponse
			{
				Balance = 1000M,
				LockedBalance = 1000M,
				TransactionId = Guid.NewGuid()
			});

		var actual = await sut.CreateWithdrawFormAsync(withdrawForm);

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormCreatedEventAsync(
				Arg.Is(actual),
				Arg.Is(brandId),
				Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task 查詢WithdrawForms給FixTool使用()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var userType = Tgs.Payment.Clients.UserType.Player;

		_ = await sut.QueryWithdrawFormsAsync(
			brandId,
			playerCode,
			userType).ToArrayAsync();

		_ = fakePaymentRepository.Received(1)
			.QueryWithdrawFormsAsync(
				Arg.Is(Tgs.Payment.WithdrawFormQueryConditions.Default with
				{
					BrandId = brandId,
					UserId = playerCode,
					UserType = Tgs.Payment.UserType.Player
				}),
				Arg.Is(false),
				Arg.Any<CancellationToken>());
	}
}
