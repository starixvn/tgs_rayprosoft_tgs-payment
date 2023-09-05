using System.Text.Json;
using NSubstitute;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;

namespace Payment.UnitTests;

public class WithdrawServiceTests
{
	[Test]
	public async Task 建立Agent的提款單()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator);

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

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormCreatedEventAsync(Arg.Is(actual), Arg.Any<Guid>());
	}

	[Test]
	public void 建立代理提款單時_如果BankInfo不是自己的會發生錯誤()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator);

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
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator);

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

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormCreatedEventAsync(Arg.Is(actual), Arg.Any<Guid>());
	}

	[Test]
	public void 建立玩家提款單_如果BankInfo不是自己的會發生錯誤()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator);

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
	public void 建立提款單時_UserType不正確時會發生錯誤()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new WithdrawService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator);

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
