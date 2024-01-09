using System.Collections.Immutable;
using Tgs.CashService.Client;
using Tgs.Payment;
using Tgs.Payment.WithdrawPipes;
using Tgs.PlayerService.Client;

namespace Payment.UnitTests.WithdrawPipeTests;

public class PlayerCashOperationPaidWithdrawFormEventPipeTests
{
	[Test]
	public async Task PlayerCashOperationPaidWithdrawFormEventPipe_完成提款後Commit玩家提款()
	{
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

		var sut = new PlayerCashOperationPaidWithdrawFormEventPipe(
			fakePlayerServiceClient,
			fakeCashServiceClient);

		var withdrawForm = new WithdrawFormSnapshot(
			Guid.NewGuid(),
			Guid.NewGuid(),
			"ThisIsPlayerCode",
			"UserName",
			UserType.Player,
			1000M,
			null,
			0M,
			null,
			PaymentFlowState.Approved,
			null,
			null,
			Guid.NewGuid(),
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			DateTime.UtcNow,
			null,
			null,
			null,
			null,
			null,
			null);
		var reviewUserId = "93a723b2-9ebb-4607-ee8d-08d978fba75f";
		var reviewUserName = "reviewedUserName";

		var walletId = Guid.NewGuid();

		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(withdrawForm.UserId))
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = walletId.ToString()
			}.ToImmutableDictionary());

		await sut.AfterPaidWithdrawFormAsync(withdrawForm, reviewUserId, reviewUserName, null);

		_ = fakeCashServiceClient.Received(1)
			.CommitWithdrawAsync(
				Arg.Is(walletId),
				Arg.Is(withdrawForm.Amount),
				Arg.Is(withdrawForm.Amount),
				Arg.Is(default(string)),
				Arg.Is(withdrawForm.Id.ToString()),
				Arg.Is(Guid.Parse(reviewUserId)));
	}

	[Test]
	public async Task PlayerCashOperationPaidWithdrawFormEventPipe_提款單如果是代理的不會動作()
	{
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

		var sut = new PlayerCashOperationPaidWithdrawFormEventPipe(
			fakePlayerServiceClient,
			fakeCashServiceClient);

		var withdrawForm = new WithdrawFormSnapshot(
			Guid.NewGuid(),
			Guid.NewGuid(),
			"ThisIsPlayerCode",
			"UserName",
			UserType.Agent,
			1000M,
			null,
			0M,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			Guid.NewGuid(),
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			DateTime.UtcNow,
			null,
			null,
			null,
			null,
			null,
			null);
		var reviewUserId = "93a723b2-9ebb-4607-ee8d-08d978fba75f";
		var reviewUserName = "reviewedUserName";

		await sut.AfterPaidWithdrawFormAsync(withdrawForm, reviewUserId, reviewUserName, null);

		_ = fakeCashServiceClient.Received(0)
			.CommitWithdrawAsync(
				Arg.Any<Guid>(),
				Arg.Any<decimal>(),
				Arg.Any<decimal>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<Guid>());
	}
}
