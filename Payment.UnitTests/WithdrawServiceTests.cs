using System.Collections.Immutable;
using System.Text.Json;
using ExpectedObjects;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Clients;
using Tgs.PlayerBucket.Client;
using Tgs.PlayerService.Client;
using Tgs.SerialNumberGenerator;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace Payment.UnitTests;

public class WithdrawServiceTests
{
	[Test]
	public async Task EmitCreateWithdrawFormEventPipe會發出表單建立事件()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[] { new EmitCreateWithdrawFormEventPipe(fakePaymentClient) });

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Agent,
			Guid.NewGuid().ToString(),
			"TestAgent");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var agentBankInfo = new AgentBankInfo(
			bankInfoId,
			Guid.Parse(user.UserId),
			brandId,
			Guid.NewGuid(),
			"Bank",
			"{}");
		_ = fakePaymentRepository.GetAgentBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(agentBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var actual = await sut.CreateWithdrawFormAsync(user, brandId, bankInfoId, amount, null);

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormCreatedEventAsync(
				Arg.Is(actual),
				Arg.Is(brandId));
	}

	[Test]
	public async Task 代理提款不會執行玩家流水檢查()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
		var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[]
			{
				new CheckPlayerBucketCreateWithdrawEventPipe(
					fakePlayerBucketClient,
					fakePlayerBucketForTypesClient)
			});

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Agent,
			Guid.NewGuid().ToString(),
			"TestAgent");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var agentBankInfo = new AgentBankInfo(
			bankInfoId,
			Guid.Parse(user.UserId),
			brandId,
			Guid.NewGuid(),
			"Bank",
			"{}");
		_ = fakePaymentRepository.GetAgentBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(agentBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		_ = await sut.CreateWithdrawFormAsync(user, brandId, bankInfoId, amount, null);

		_ = fakePlayerBucketClient.Received(0)
			.GetBrandBucketTypesAsync(Arg.Any<Guid>());
		_ = fakePlayerBucketForTypesClient.Received(0)
			.ReadBucketsForBucketTypesAsync(
				Arg.Any<string>(),
				Arg.Any<IEnumerable<string>>());
	}

	[Test]
	public async Task 代理提款不會執行玩家錢包動作()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakeSubmitWithdrawFormStatusCommandService = Substitute.For<ICommandService<SubmitWithdrawFormStatusCommand>>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[]
			{
				new PlayerCashOperationCreateWithdrawFormEventPipe(
					fakePlayerServiceClient,
					fakeCashServiceClient,
					fakeSubmitWithdrawFormStatusCommandService)
			});

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Agent,
			Guid.NewGuid().ToString(),
			"TestAgent");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var agentBankInfo = new AgentBankInfo(
			bankInfoId,
			Guid.Parse(user.UserId),
			brandId,
			Guid.NewGuid(),
			"Bank",
			"{}");
		_ = fakePaymentRepository.GetAgentBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(agentBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		_ = await sut.CreateWithdrawFormAsync(user, brandId, bankInfoId, amount, null);

		_ = fakeCashServiceClient.Received(0)
			.GetBalanceAsync(Arg.Any<Guid>());
		_ = fakeCashServiceClient.Received(0)
			.LockWithdrawAsync(
				Arg.Any<Guid>(),
				Arg.Any<decimal>(),
				Arg.Any<string>(),
				Arg.Any<string>());
	}

	[Test]
	public void 玩家提款時扣款後錢包餘額為負數會Rollback交易_並且將表單拒絕掉()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakeSubmitWithdrawFormStatusCommandService = Substitute.For<ICommandService<SubmitWithdrawFormStatusCommand>>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[] {
				new PlayerCashOperationCreateWithdrawFormEventPipe(
					fakePlayerServiceClient,
					fakeCashServiceClient,
					fakeSubmitWithdrawFormStatusCommandService)
			});

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Player,
			"ThisIsPlayerCode",
			"TestUser");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var playerBankInfo = new PlayerBankInfo(
			bankInfoId,
			user.UserId,
			brandId,
			Guid.NewGuid(),
			"Bank",
			JsonDocument.Parse("{}"),
			false);
		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(playerBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var walletId = Guid.NewGuid();
		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(user.UserId))
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = walletId.ToString()
			}.ToImmutableDictionary());

		_ = fakeCashServiceClient.GetBalanceAsync(Arg.Is(walletId))
			.Returns(1000M);

		_ = fakeCashServiceClient.LockWithdrawAsync(
			Arg.Is(walletId),
			Arg.Is(amount),
			Arg.Any<string>(),
			Arg.Any<string>())
			.Returns(new TransactionResponse
			{
				Balance = -100M,
				LockedBalance = 1000M,
				TransactionId = Guid.NewGuid()
			});

		_ = Assert.ThrowsAsync<InsufficientFundsException>(
			async () => await sut.CreateWithdrawFormAsync(
				user,
				brandId,
				bankInfoId,
				amount,
				null));

		_ = fakeCashServiceClient.Received(1)
			.RollbackWithdrawAsync(
				Arg.Is(walletId),
				Arg.Is(amount),
				Arg.Is(default(string)),
				Arg.Any<string>(),
				Arg.Is(default(Guid?)));

		var expectedCommand = new
		{
			Status = PaymentFlowState.Reject
		};
		_ = fakeSubmitWithdrawFormStatusCommandService.Received(1)
			.ExecuteAsync(Arg.Is<SubmitWithdrawFormStatusCommand>(
				cmd => expectedCommand.ToExpectedObject().Matches(cmd)));
	}

	[Test]
	public void 玩家提款時流水如果沒清空不能提款()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
		var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[] {
				new CheckPlayerBucketCreateWithdrawEventPipe(
					fakePlayerBucketClient,
					fakePlayerBucketForTypesClient)
			});

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Player,
			"ThisIsPlayerCode",
			"TestUser");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var playerBankInfo = new PlayerBankInfo(
			bankInfoId,
			user.UserId,
			brandId,
			Guid.NewGuid(),
			"Bank",
			JsonDocument.Parse("{}"),
			false);
		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(playerBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		_ = fakePlayerBucketClient.GetBrandBucketTypesAsync(Arg.Is(brandId))
			.Returns(new[] { "Game1", "Game2" }.ToAsyncEnumerable());

		_ = fakePlayerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
			Arg.Is(user.UserId),
			Arg.Any<IEnumerable<string>>())
			.Returns(new[]
			{
				new TypeBucketResult(
					null,
					new ReadBucketResult(0M, null)),
				new TypeBucketResult(
					"Game1",
					new ReadBucketResult(10M, null)),
				new TypeBucketResult(
					"Game2",
					new ReadBucketResult(0M, null))
			}.ToAsyncEnumerable());

		_ = Assert.ThrowsAsync<InsufficientBucketException>(
			async () => await sut.CreateWithdrawFormAsync(user, brandId, bankInfoId, amount, null));
	}

	[Test]
	public async Task 玩家提款時流水都清空則可以建立表單()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
		var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[] {
				new CheckPlayerBucketCreateWithdrawEventPipe(
					fakePlayerBucketClient,
					fakePlayerBucketForTypesClient)
			});

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Player,
			"ThisIsPlayerCode",
			"TestUser");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var playerBankInfo = new PlayerBankInfo(
			bankInfoId,
			user.UserId,
			brandId,
			Guid.NewGuid(),
			"Bank",
			JsonDocument.Parse("{}"),
			false);
		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(playerBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		_ = fakePlayerBucketClient.GetBrandBucketTypesAsync(Arg.Is(brandId))
			.Returns(new[] { "Game1", "Game2" }.ToAsyncEnumerable());

		_ = fakePlayerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
			Arg.Is(user.UserId),
			Arg.Any<IEnumerable<string>>())
			.Returns(new[]
			{
				new TypeBucketResult(
					null,
					new ReadBucketResult(0M, null)),
				new TypeBucketResult(
					"Game1",
					new ReadBucketResult(0M, null)),
				new TypeBucketResult(
					"Game2",
					new ReadBucketResult(0M, null))
			}.ToAsyncEnumerable());

		var actual = await sut.CreateWithdrawFormAsync(user, brandId, bankInfoId, amount, null);

		Assert.AreNotEqual(Guid.Empty, actual);

		var expectedSnapshot = new WithdrawFormSnapshot(
			actual,
			brandId,
			user.UserId,
			user.UserName,
			user.UserType,
			amount,
			null,
			0M,
			null,
			PaymentFlowState.Init,
			null,
			null,
			bankInfoId,
			JsonSerializer.Serialize(new WithdrawBankInfo<JsonDocument>(
				playerBankInfo.Id,
				playerBankInfo.PaymentChannelId,
				playerBankInfo.BankType,
				playerBankInfo.Detail), PaymentUtility.SerializerOptions),
			null,
			null,
			orderNo,
			null,
			currentTime.UtcDateTime,
			null,
			null,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.Received(1)
			.GetBrandInfoByIdAsync(Arg.Is(brandId));

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(actual),
				Arg.Is(EventAction.Create),
				Arg.Any<JsonDocument>(),
				Arg.Any<string>());

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));
	}

	[Test]
	public void 玩家提款時錢包餘額不足會建立表單失敗()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakeSubmitWithdrawFormStatusCommandService = Substitute.For<ICommandService<SubmitWithdrawFormStatusCommand>>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[] {
				new PlayerCashOperationCreateWithdrawFormEventPipe(
					fakePlayerServiceClient,
					fakeCashServiceClient,
					fakeSubmitWithdrawFormStatusCommandService)
			});

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Player,
			"ThisIsPlayerCode",
			"TestUser");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var playerBankInfo = new PlayerBankInfo(
			bankInfoId,
			user.UserId,
			brandId,
			Guid.NewGuid(),
			"Bank",
			JsonDocument.Parse("{}"),
			false);
		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(playerBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var walletId = Guid.NewGuid();
		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(user.UserId))
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = walletId.ToString()
			}.ToImmutableDictionary());

		_ = fakeCashServiceClient.GetBalanceAsync(Arg.Is(walletId))
			.Returns(500M);

		_ = Assert.ThrowsAsync<InsufficientFundsException>(
			async () => await sut.CreateWithdrawFormAsync(user, brandId, bankInfoId, amount, null));

		_ = fakeCashServiceClient.Received(0)
			.LockWithdrawAsync(
				Arg.Any<Guid>(),
				Arg.Any<decimal>(),
				Arg.Any<string>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task 玩家提款單建立前會先檢查錢包餘額是否足夠_建立完成後_會進行扣款()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakeSubmitWithdrawFormStatusCommandService = Substitute.For<ICommandService<SubmitWithdrawFormStatusCommand>>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[] {
				new PlayerCashOperationCreateWithdrawFormEventPipe(
					fakePlayerServiceClient,
					fakeCashServiceClient,
					fakeSubmitWithdrawFormStatusCommandService)
			});

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Player,
			"ThisIsPlayerCode",
			"TestUser");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var playerBankInfo = new PlayerBankInfo(
			bankInfoId,
			user.UserId,
			brandId,
			Guid.NewGuid(),
			"Bank",
			JsonDocument.Parse("{}"),
			false);
		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(playerBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var walletId = Guid.NewGuid();
		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(user.UserId))
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = walletId.ToString()
			}.ToImmutableDictionary());

		_ = fakeCashServiceClient.GetBalanceAsync(Arg.Is(walletId))
			.Returns(1000M);

		_ = fakeCashServiceClient.LockWithdrawAsync(
			Arg.Is(walletId),
			Arg.Is(amount),
			Arg.Is(default(string)),
			Arg.Any<string>())
			.Returns(new TransactionResponse
			{
				Balance = 0M,
				LockedBalance = 1000M,
				TransactionId = Guid.NewGuid()
			});

		var actual = await sut.CreateWithdrawFormAsync(user, brandId, bankInfoId, amount, null);

		_ = fakeCashServiceClient.Received(1)
			.LockWithdrawAsync(
			Arg.Is(walletId),
			Arg.Is(amount),
			Arg.Is(default(string)),
			Arg.Is(actual.ToString()));
	}

	[Test]
	public async Task 建立Agent的提款單()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			Array.Empty<ICreateWithdrawFormPipe>());

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Agent,
			Guid.NewGuid().ToString(),
			"TestAgent");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var agentBankInfo = new AgentBankInfo(
			bankInfoId,
			Guid.Parse(user.UserId),
			brandId,
			Guid.NewGuid(),
			"Bank",
			"{}");
		_ = fakePaymentRepository.GetAgentBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(agentBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var actual = await sut.CreateWithdrawFormAsync(user, brandId, bankInfoId, amount, null);

		Assert.AreNotEqual(Guid.Empty, actual);

		var expectedSnapshot = new WithdrawFormSnapshot(
			actual,
			brandId,
			user.UserId,
			user.UserName,
			user.UserType,
			amount,
			null,
			0M,
			null,
			PaymentFlowState.Init,
			null,
			null,
			bankInfoId,
			JsonSerializer.Serialize(new WithdrawBankInfo<JsonDocument>(
				agentBankInfo.Id,
				agentBankInfo.PaymentChannelId,
				agentBankInfo.BankType,
				JsonDocument.Parse(agentBankInfo.Detail)), PaymentUtility.SerializerOptions),
			null,
			null,
			orderNo,
			null,
			currentTime.UtcDateTime,
			null,
			null,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.Received(1)
			.GetBrandInfoByIdAsync(Arg.Is(brandId));

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(actual),
				Arg.Is(EventAction.Create),
				Arg.Any<JsonDocument>(),
				Arg.Any<string>());

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));
	}

	[Test]
	public void 建立代理提款單時_如果BankInfo不是自己的會發生錯誤()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			Array.Empty<ICreateWithdrawFormPipe>());

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Agent,
			Guid.NewGuid().ToString(),
			"TestAgent");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		_ = fakePaymentRepository.GetAgentBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(new AgentBankInfo(
				bankInfoId,
				Guid.NewGuid(),
				brandId,
				Guid.NewGuid(),
				"Bank",
				"{}"));

		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.CreateWithdrawFormAsync(
			user,
			brandId,
			bankInfoId,
			amount,
			null));
	}

	[Test]
	public async Task 建立玩家的提款單()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			Array.Empty<ICreateWithdrawFormPipe>());

		var brandId = Guid.NewGuid();

		var user = new WithdrawUser(
			UserType.Player,
			"ThisIsPlayerCode",
			"TestUser");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;
		var note = "ThisIsPlayerNote";

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var playerBankInfo = new PlayerBankInfo(
			bankInfoId,
			user.UserId,
			brandId,
			Guid.NewGuid(),
			"Bank",
			JsonDocument.Parse("{}"),
			false);
		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(playerBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var actual = await sut.CreateWithdrawFormAsync(user, brandId, bankInfoId, amount, note);

		Assert.AreNotEqual(Guid.Empty, actual);

		var expectedSnapshot = new WithdrawFormSnapshot(
			actual,
			brandId,
			user.UserId,
			user.UserName,
			user.UserType,
			amount,
			null,
			0M,
			null,
			PaymentFlowState.Init,
			null,
			note,
			bankInfoId,
			JsonSerializer.Serialize(new WithdrawBankInfo<JsonDocument>(
				playerBankInfo.Id,
				playerBankInfo.PaymentChannelId,
				playerBankInfo.BankType,
				playerBankInfo.Detail), PaymentUtility.SerializerOptions),
			null,
			null,
			orderNo,
			null,
			currentTime.UtcDateTime,
			null,
			null,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.Received(1)
			.GetBrandInfoByIdAsync(Arg.Is(brandId));

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(actual),
				Arg.Is(EventAction.Create),
				Arg.Any<JsonDocument>(),
				Arg.Any<string>());

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));
	}

	[Test]
	public void 建立玩家提款單_如果BankInfo不是自己的會發生錯誤()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			Array.Empty<ICreateWithdrawFormPipe>());

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Player,
			"ThisIsPlayerCode",
			"TestUser");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(new PlayerBankInfo(
				bankInfoId,
				"OtherPlayerCode",
				brandId,
				Guid.NewGuid(),
				"Bank",
				JsonDocument.Parse("{}"),
				false));

		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.CreateWithdrawFormAsync(
			user,
			brandId,
			bankInfoId,
			amount,
			null));
	}

	[Test]
	public async Task 建立提款單時_Pipe的After會依照Order由小到大的順序執行()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePipe1 = Substitute.For<ICreateWithdrawFormPipe>();
		var fakePipe2 = Substitute.For<ICreateWithdrawFormPipe>();
		var fakePipe3 = Substitute.For<ICreateWithdrawFormPipe>();

		_ = fakePipe3.Order.Returns(100);
		_ = fakePipe2.Order.Returns(1);
		_ = fakePipe1.Order.Returns(10);

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[] { fakePipe1, fakePipe2, fakePipe3 });

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Agent,
			Guid.NewGuid().ToString(),
			"TestAgent");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var agentBankInfo = new AgentBankInfo(
			bankInfoId,
			Guid.Parse(user.UserId),
			brandId,
			Guid.NewGuid(),
			"Bank",
			"{}");
		_ = fakePaymentRepository.GetAgentBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(agentBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var actual = await sut.CreateWithdrawFormAsync(
			user,
			brandId,
			bankInfoId,
			amount,
			null);

		Received.InOrder(() =>
		{
			_ = fakePipe2.AfterCreateWithdrawFormAsync(
				Arg.Is(user),
				Arg.Is(brandId),
				Arg.Is(bankInfoId),
				Arg.Is(amount),
				Arg.Is(default(string)),
				Arg.Is(actual));
			_ = fakePipe1.AfterCreateWithdrawFormAsync(
				Arg.Is(user),
				Arg.Is(brandId),
				Arg.Is(bankInfoId),
				Arg.Is(amount),
				Arg.Is(default(string)),
				Arg.Is(actual));
			_ = fakePipe3.AfterCreateWithdrawFormAsync(
				Arg.Is(user),
				Arg.Is(brandId),
				Arg.Is(bankInfoId),
				Arg.Is(amount),
				Arg.Is(default(string)),
				Arg.Is(actual));
		});
	}

	[Test]
	public async Task 建立提款單時_Pipe的Before會依照Order由大到小的順序執行()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var fakePipe1 = Substitute.For<ICreateWithdrawFormPipe>();
		var fakePipe2 = Substitute.For<ICreateWithdrawFormPipe>();
		var fakePipe3 = Substitute.For<ICreateWithdrawFormPipe>();

		_ = fakePipe3.Order.Returns(100);
		_ = fakePipe2.Order.Returns(1);
		_ = fakePipe1.Order.Returns(10);

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			new[] { fakePipe1, fakePipe2, fakePipe3 });

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.Agent,
			Guid.NewGuid().ToString(),
			"TestAgent");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var agentBankInfo = new AgentBankInfo(
			bankInfoId,
			Guid.Parse(user.UserId),
			brandId,
			Guid.NewGuid(),
			"Bank",
			"{}");
		_ = fakePaymentRepository.GetAgentBankInfoByIdAsync(Arg.Is(bankInfoId))
			.Returns(agentBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		_ = await sut.CreateWithdrawFormAsync(
			user,
			brandId,
			bankInfoId,
			amount,
			null);

		Received.InOrder(() =>
		{
			_ = fakePipe3.BeforeCreateWithdrawFormAsync(
				Arg.Is(user),
				Arg.Is(brandId),
				Arg.Is(bankInfoId),
				Arg.Is(amount),
				Arg.Is(default(string)));
			_ = fakePipe1.BeforeCreateWithdrawFormAsync(
				Arg.Is(user),
				Arg.Is(brandId),
				Arg.Is(bankInfoId),
				Arg.Is(amount),
				Arg.Is(default(string)));
			_ = fakePipe2.BeforeCreateWithdrawFormAsync(
				Arg.Is(user),
				Arg.Is(brandId),
				Arg.Is(bankInfoId),
				Arg.Is(amount),
				Arg.Is(default(string)));
		});
	}

	[Test]
	public void 建立提款單時_UserType不正確時會發生錯誤()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator,
			Array.Empty<ICreateWithdrawFormPipe>());

		var brandId = Guid.NewGuid();
		var user = new WithdrawUser(
			UserType.None,
			"Unknown",
			"Unknown");
		var bankInfoId = Guid.NewGuid();
		var amount = 1000M;

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.CreateWithdrawFormAsync(
			user,
			brandId,
			bankInfoId,
			amount,
			null));
	}
}
