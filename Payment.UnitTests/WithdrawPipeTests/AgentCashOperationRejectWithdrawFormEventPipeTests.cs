using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using Tgs.CashService.Client;
using Tgs.Payment;
using Tgs.Payment.WithdrawPipes;

namespace Payment.UnitTests.WithdrawPipeTests;

public class AgentCashOperationRejectWithdrawFormEventPipeTests
{
	[Test]
	public async Task AgentCashOperationRejectWithdrawFormEventPipe_拒絕提款單後Rollback代理提款金額()
	{
		var fakeAgentClient = Substitute.For<IAgentClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

		var sut = new AgentCashOperationRejectWithdrawFormEventPipe(
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
				false));

		await sut.AfterRejectWithdrawFormAsync(withdrawForm, reviewUserId, reviewUserName, null);

		_ = fakeCashServiceClient.Received(1)
			.RollbackWithdrawAsync(
				Arg.Is(walletId),
				Arg.Is(withdrawForm.Amount),
				Arg.Is(default(string)),
				Arg.Is(withdrawForm.Id.ToString()),
				Arg.Is(Guid.Parse(reviewUserId)));
	}

	[Test]
	public async Task AgentCashOperationRejectWithdrawFormEventPipe_如果提款單是玩家的不會進行動作()
	{
		var fakeAgentClient = Substitute.For<IAgentClient>();
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

		var sut = new AgentCashOperationRejectWithdrawFormEventPipe(
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

		await sut.AfterRejectWithdrawFormAsync(withdrawForm, reviewUserId, reviewUserName, null);

		_ = fakeCashServiceClient.Received(0)
			.RollbackWithdrawAsync(
				Arg.Any<Guid>(),
				Arg.Any<decimal>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<Guid>());
	}
}
