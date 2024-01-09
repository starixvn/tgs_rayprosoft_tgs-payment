using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.CashService.Client;

namespace Tgs.Payment.WithdrawPipes;

internal class AgentCashOperationRejectWithdrawFormEventPipe(
	IAgentClient agentClient,
	ICashServiceClient cashServiceClient)
	: IRejectWithdrawFormPipe
{
	public async ValueTask AfterRejectWithdrawFormAsync(
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default)
	{
		if (formData.UserType is not UserType.Agent)
			return;

		var agentId = Guid.Parse(formData.UserId!);

		var agent = await agentClient.GetAgentInfoAsync(
			new GetAgentInfoRequest(agentId),
			cancellationToken).ConfigureAwait(false);

		var walletId = agent!.WalletId!.Value;

		_ = await cashServiceClient.RollbackWithdrawAsync(
			walletId,
			formData.Amount,
			null,
			formData.Id.ToString(),
			string.IsNullOrWhiteSpace(reviewUserId) ? Guid.Empty : Guid.Parse(reviewUserId),
			cancellationToken).ConfigureAwait(false);
	}
}
