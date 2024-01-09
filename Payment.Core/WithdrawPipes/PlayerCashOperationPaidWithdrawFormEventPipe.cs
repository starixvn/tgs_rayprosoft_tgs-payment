using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;

namespace Tgs.Payment.WithdrawPipes;

internal class PlayerCashOperationPaidWithdrawFormEventPipe(
	IPlayerServiceClient playerServiceClient,
	ICashServiceClient cashServiceClient) : IPaidWithdrawFormPipe
{
	public async ValueTask AfterPaidWithdrawFormAsync(
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default)
	{
		if (formData.UserType is not UserType.Player)
			return;

		var playerProps = await playerServiceClient.GetPlayerPropertiesAsync(
			formData.UserId!,
			cancellationToken).ConfigureAwait(false);

		var walletId = Guid.Parse(playerProps["DefaultWalletId"]);

		_ = await cashServiceClient.CommitWithdrawAsync(
			walletId,
			formData.Amount,
			formData.Amount,
			null,
			formData.Id.ToString(),
			string.IsNullOrWhiteSpace(reviewUserId) ? Guid.Empty : Guid.Parse(reviewUserId),
			cancellationToken).ConfigureAwait(false);
	}
}
