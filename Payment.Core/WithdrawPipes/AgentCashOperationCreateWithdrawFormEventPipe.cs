using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.CashService.Client;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.ServiceUtility;

namespace Tgs.Payment.WithdrawPipes;

internal class AgentCashOperationCreateWithdrawFormEventPipe(
	IAgentClient agentClient,
	ICashServiceClient cashServiceClient,
	ICommandService<SubmitWithdrawFormStatusCommand> submitWithdrawFormStatusService)
	: ICreateWithdrawFormPipe
{
	public int Order => 1;

	public async ValueTask AfterCreateWithdrawFormAsync(
		WithdrawUser user,
		Guid brandId,
		Guid bankInfoId,
		decimal amount,
		string? note,
		Guid createFormId,
		CancellationToken cancellationToken = default)
	{
		if (user.UserType is not UserType.Agent)
			return;

		var agent = await agentClient.GetAgentInfoAsync(
			new Agent.Client.Models.GetAgentInfoRequest(Guid.Parse(user.UserId!)),
			cancellationToken).ConfigureAwait(false);

		var agentWalletId = agent!.WalletId!.Value;

		var result = await cashServiceClient.LockWithdrawAsync(
			agentWalletId,
			amount,
			null,
			createFormId.ToString(),
			cancellationToken).ConfigureAwait(false);

		if (result.Balance < 0)
		{
			_ = await cashServiceClient.RollbackWithdrawAsync(
				agentWalletId,
				amount,
				null,
				createFormId.ToString(),
				null,
				cancellationToken).ConfigureAwait(false);

			var command = new SubmitWithdrawFormStatusCommand(
				createFormId,
				PaymentFlowState.Reject,
				null,
				null,
				null,
				null,
				null);

			await submitWithdrawFormStatusService.ExecuteAsync(
				command,
				cancellationToken).ConfigureAwait(false);

			throw new InsufficientFundsException();
		}
	}

	public async ValueTask BeforeCreateWithdrawFormAsync(
		WithdrawUser user,
		Guid brandId,
		Guid bankInfoId,
		decimal amount,
		string? note,
		CancellationToken cancellationToken = default)
	{
		if (user.UserType is not UserType.Agent)
			return;

		var agent = await agentClient.GetAgentInfoAsync(
			new Agent.Client.Models.GetAgentInfoRequest(Guid.Parse(user.UserId!)),
			cancellationToken).ConfigureAwait(false);

		var balance = await cashServiceClient
			.GetBalanceAsync(agent!.WalletId!.Value, cancellationToken)
			.ConfigureAwait(false);

		if (balance - amount < 0)
			throw new InsufficientFundsException();
	}
}
