using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.BrandCertificate.Clients;
using Tgs.CashService.Client;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using Tgs.Payment.WithdrawLimit;
using Tgs.Payment.WithdrawLimit.Clients;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;

namespace Payment.IntegrationTests;

public class WithdrawFormTests
{
	[Test]
	public async Task 玩家提款時會檢查Vip當日限額()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
		var fakeVipLevelClient = Substitute.For<IVipLevelServiceClient>();
		var fakeWithdrawLimitClient = Substitute.For<IWithdrawLimitClient>();
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePlayerServiceClient)
				.AddSingleton(fakeCashServiceClient)
				.AddSingleton(fakePlayerBucketClient)
				.AddSingleton(fakeVipLevelClient)
				.AddSingleton(fakeWithdrawLimitClient)
				.AddSingleton(fakeBrandServiceClient)
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

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Id = brandId,
				Code = "GPI"
			});

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

		_ = fakePlayerBucketClient.ReadBucketsAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(new ReadBucketResult(0M, null));

		_ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
			.Returns(new Tgs.PlayerService.Client.PlayerInfo
			{
				PlayerCode = playerCode,
				BrandId = brandId
			});

		_ = fakeVipLevelClient.GetBrandAllVipLevelsAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new[]
			{
				new Tgs.VipLevelService.Clients.VipLevelInfo(
					Guid.NewGuid(),
					1,
					brandId,
					0M,
					0M,
					0M,
					0M,
					true,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					5,
					10000M)
			}.ToAsyncEnumerable());

		_ = fakeWithdrawLimitClient.IncreasePlayerWithdrawStatisticsAsync(
			Arg.Is(playerCode),
			Arg.Any<DateOnly>(),
			Arg.Is(withdrawForm.Amount),
			Arg.Any<CancellationToken>())
			.Returns(new WithdrawStatistics(11000M, 6));

		_ = Assert.ThrowsAsync<Tgs.Payment.Clients.WithdrawLimitExceedException>(
			async () => await sut.CreateWithdrawFormAsync(withdrawForm));

		_ = fakeWithdrawLimitClient.Received(1)
			.DecreasePlayerWithdrawStatisticsAsync(
				Arg.Is(playerCode),
				Arg.Any<DateOnly>(),
				Arg.Is(withdrawForm.Amount),
				Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task 玩家提款時會檢查流水是否已經清空()
	{
		var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePlayerBucketClient)
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

		_ = fakePlayerBucketClient.GetBrandBucketTypesAsync(
			Arg.Is(brandId),
			Arg.Any<CancellationToken>())
			.Returns(AsyncEnumerable.Empty<string>());
		_ = fakePlayerBucketClient.ReadBucketsAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(new ReadBucketResult(100M, null));

		_ = Assert.ThrowsAsync<Tgs.Payment.Clients.InsufficientBucketException>(
			async () => await sut.CreateWithdrawFormAsync(withdrawForm));
	}

	[Test]
	public async Task 玩家提款時會檢查錢包餘額是否足夠()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
		var fakeVipLevelClient = Substitute.For<IVipLevelServiceClient>();
		var fakeWithdrawLimitClient = Substitute.For<IWithdrawLimitClient>();
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePlayerServiceClient)
				.AddSingleton(fakeCashServiceClient)
				.AddSingleton(fakePlayerBucketClient)
				.AddSingleton(fakeVipLevelClient)
				.AddSingleton(fakeWithdrawLimitClient)
				.AddSingleton(fakeBrandServiceClient)
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

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Id = brandId,
				Code = "GPI"
			});

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

		_ = fakePlayerBucketClient.ReadBucketsAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(new ReadBucketResult(0M, null));

		_ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
			.Returns(new Tgs.PlayerService.Client.PlayerInfo
			{
				PlayerCode = playerCode,
				BrandId = brandId
			});

		_ = fakeVipLevelClient.GetBrandAllVipLevelsAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new[]
			{
				new Tgs.VipLevelService.Clients.VipLevelInfo(
					Guid.NewGuid(),
					1,
					brandId,
					0M,
					0M,
					0M,
					0M,
					true,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					5,
					10000M)
			}.ToAsyncEnumerable());

		_ = fakeWithdrawLimitClient.IncreasePlayerWithdrawStatisticsAsync(
			Arg.Is(playerCode),
			Arg.Any<DateOnly>(),
			Arg.Is(withdrawForm.Amount),
			Arg.Any<CancellationToken>())
			.Returns(new WithdrawStatistics(1000M, 1));

		_ = Assert.ThrowsAsync<Tgs.Payment.Clients.InsufficientFundsException>(
			async () => await sut.CreateWithdrawFormAsync(withdrawForm));

		_ = fakeCashServiceClient.Received(1)
			.GetBalanceAsync(
				Arg.Is(walletId),
				Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task 玩家提款到達限額時會發出WithdrawLimitExceedException()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
		var fakeVipLevelClient = Substitute.For<IVipLevelServiceClient>();
		var fakeWithdrawLimitClient = Substitute.For<IWithdrawLimitClient>();
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePlayerServiceClient)
				.AddSingleton(fakeCashServiceClient)
				.AddSingleton(fakePlayerBucketClient)
				.AddSingleton(fakeVipLevelClient)
				.AddSingleton(fakeWithdrawLimitClient)
				.AddSingleton(fakeBrandServiceClient)
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

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Id = brandId,
				Code = "GPI"
			});

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

		_ = fakePlayerBucketClient.ReadBucketsAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(new ReadBucketResult(0M, null));

		_ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
			.Returns(new Tgs.PlayerService.Client.PlayerInfo
			{
				PlayerCode = playerCode,
				BrandId = brandId
			});

		_ = fakeVipLevelClient.GetBrandAllVipLevelsAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new[]
			{
				new Tgs.VipLevelService.Clients.VipLevelInfo(
					Guid.NewGuid(),
					1,
					brandId,
					0M,
					0M,
					0M,
					0M,
					true,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					5,
					100M)
			}.ToAsyncEnumerable());

		_ = fakeWithdrawLimitClient.IncreasePlayerWithdrawStatisticsAsync(
			Arg.Is(playerCode),
			Arg.Any<DateOnly>(),
			Arg.Is(withdrawForm.Amount),
			Arg.Any<CancellationToken>())
			.Returns(new WithdrawStatistics(1000M, 1));

		_ = Assert.ThrowsAsync<Tgs.Payment.Clients.WithdrawLimitExceedException>(
			async () => await sut.CreateWithdrawFormAsync(withdrawForm));
	}

	[Test]
	public async Task 建立WithdrawForm會發出Event()
	{
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
		var fakeVipLevelClient = Substitute.For<IVipLevelServiceClient>();
		var fakeWithdrawLimitClient = Substitute.For<IWithdrawLimitClient>();
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePaymentClient)
				.AddSingleton(fakePlayerServiceClient)
				.AddSingleton(fakeCashServiceClient)
				.AddSingleton(fakePlayerBucketClient)
				.AddSingleton(fakeVipLevelClient)
				.AddSingleton(fakeWithdrawLimitClient)
				.AddSingleton(fakeBrandServiceClient));

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

		_ = await dbContext.SaveChangesAsync();

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Id = brandId,
				Code = "GPI"
			});

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

		_ = fakePlayerBucketClient.ReadBucketsAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(new ReadBucketResult(0M, null));

		_ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
			.Returns(new Tgs.PlayerService.Client.PlayerInfo
			{
				PlayerCode = playerCode,
				BrandId = brandId
			});

		_ = fakeVipLevelClient.GetBrandAllVipLevelsAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new[]
			{
				new Tgs.VipLevelService.Clients.VipLevelInfo(
					Guid.NewGuid(),
					1,
					brandId,
					0M,
					0M,
					0M,
					0M,
					true,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					5,
					10000M)
			}.ToAsyncEnumerable());

		_ = fakeWithdrawLimitClient.IncreasePlayerWithdrawStatisticsAsync(
			Arg.Is(playerCode),
			Arg.Any<DateOnly>(),
			Arg.Is(withdrawForm.Amount),
			Arg.Any<CancellationToken>())
			.Returns(new WithdrawStatistics(1000M, 1));

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

	[Test]
	public async Task 查詢當日前十大提款玩家()
	{
		var fakeService = Substitute.For<IQueryService<GetCurrentDayTopTenWithdrawPlayersQuery, Tgs.Payment.TopTenDepositWithdrawInfo>>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services
			=> services.AddSingleton(fakeService));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

		_ = fakeService
			.QueryAsync(new GetCurrentDayTopTenWithdrawPlayersQuery(brandId), Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.TopTenDepositWithdrawInfo[] {
				new() {
					PlayerCode = "playerCode",
					DepositTotalAmount = 2000m,
					WithdrawTotalAmount = 2000m
				}
			}.ToAsyncEnumerable());

		var actual = await sut.GetCurrentDayTopTenWithdrawPlayersAsync(brandId).ToArrayAsync();

		ClassicAssert.AreEqual(1, actual.Length);
		ClassicAssert.AreEqual("playerCode", actual[0].PlayerCode);
		ClassicAssert.AreEqual(2000m, actual[0].DepositTotalAmount);
		ClassicAssert.AreEqual(2000m, actual[0].WithdrawTotalAmount);
	}
}
