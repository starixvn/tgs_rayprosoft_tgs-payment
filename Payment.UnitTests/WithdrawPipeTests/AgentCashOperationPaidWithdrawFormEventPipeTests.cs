using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using Tgs.CashService.Client;
using Tgs.Payment;
using Tgs.Payment.WithdrawPipes;

namespace Payment.UnitTests.WithdrawPipeTests;

public class AgentCashOperationPaidWithdrawFormEventPipeTests
{
	[Test]
	public async Task AgentCashOperationPaidWithdrawFormEventPipe_完成提款後Commit代理提款金額()
	{
		var fakeAgentClient = Substitute.For<IAgentClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

		var sut = new AgentCashOperationPaidWithdrawFormEventPipe(
			fakeAgentClient,
			fakeCashServiceClient);

		var agentId = Guid.NewGuid();
		var withdrawForm = new WithdrawFormSnapshot(
			Guid.NewGuid(),
			Guid.NewGuid(),
			agentId.ToString(),
			"UserName",
			UserType.Agent,
			1000M,
			null,
			0M,
			null,
			PaymentFlowState.Approved,
			null,
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

		_ = fakeAgentClient.GetAgentInfoAsync(Arg.Is(new GetAgentInfoRequest(agentId)))
			.Returns(new AgentInfoModel(
				agentId,
				"test",
				null,
				null,
				null,
				null,
				null,
				"aa1234",
				null,
				DateTimeOffset.UtcNow,
				AgentStatus.Enabled,
				AgentType.Cash,
				AgentLevelType.Agent,
				0M,
				0,
				0M,
				Guid.NewGuid(),
				Guid.NewGuid(),
				walletId,
				false,
				false));

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
	public async Task AgentCashOperationPaidWithdrawFormEventPipe_如果提款單是玩家的不會進行動作()
	{
		var fakeAgentClient = Substitute.For<IAgentClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

		var sut = new AgentCashOperationPaidWithdrawFormEventPipe(
			fakeAgentClient,
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
			PaymentFlowState.WaitApprove,
			null,
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
