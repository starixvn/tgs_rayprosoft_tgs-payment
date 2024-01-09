using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.Payment.WithdrawPipes;

internal class PlayerCashOperationCreateWithdrawFormEventPipe(
	IPlayerServiceClient playerServiceClient,
	ICashServiceClient cashServiceClient,
	ICommandService<SubmitWithdrawFormStatusCommand> submitWithdrawFormStatusService) : ICreateWithdrawFormPipe
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
		if (user.UserType is not UserType.Player)
			return;

		var playerProps = await playerServiceClient
			.GetPlayerPropertiesAsync(user.UserId!, cancellationToken)
			.ConfigureAwait(false);

		var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]!);

		var result = await cashServiceClient.LockWithdrawAsync(
			playerWalletId,
			amount,
			null,
			createFormId.ToString(),
			cancellationToken).ConfigureAwait(false);

		if (result.Balance < 0)
		{
			_ = await cashServiceClient.RollbackWithdrawAsync(
				playerWalletId,
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
		if (user.UserType is not UserType.Player)
			return;

		var playerProps = await playerServiceClient
			.GetPlayerPropertiesAsync(user.UserId!, cancellationToken)
			.ConfigureAwait(false);

		var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]!);

		var balance = await cashServiceClient
			.GetBalanceAsync(playerWalletId, cancellationToken)
			.ConfigureAwait(false);

		if (balance - amount < 0)
			throw new InsufficientFundsException();
	}
}
