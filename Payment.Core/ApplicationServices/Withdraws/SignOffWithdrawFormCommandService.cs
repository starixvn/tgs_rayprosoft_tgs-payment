using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class SignOffWithdrawFormCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient,
	IWithdrawService withdrawService,
	ILogger<SignOffWithdrawFormCommandService> logger)
	: ICommandService<SignOffWithdrawFormCommand>
{
	public async ValueTask ExecuteAsync(SignOffWithdrawFormCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await paymentRepository
			.GetCurrentWithdrawFormSnapshotAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData is null)
			return;

		var formStatus = formData.Status;

		logger.LogDebug("SignOffWithdrawFormCommand command:{command}, formData Status:{formStatus}",
			JsonSerializer.Serialize(command, PaymentUtility.SerializerOptions),
			formStatus);

		switch (command.Status)
		{
			case ReviewState.Approve:
				await withdrawService.ApproveWithdrawFormAsync(
					command.BrandPaymentId!.Value,
					formData,
					command.ReviewUserId,
					command.ReviewUserName,
					command.Remark,
					command.PlayerRemark,
					cancellationToken).ConfigureAwait(false);
				break;

			case ReviewState.Paid:
				await withdrawService.PaidWithdrawFormAsync(
					formData,
					command.ReviewUserId,
					command.ReviewUserName,
					command.Remark,
					command.PlayerRemark,
					cancellationToken).ConfigureAwait(false);
				break;

			default:
				await withdrawService.RejectWithdrawFormAsync(
					formData,
					command.ReviewUserId,
					command.ReviewUserName,
					command.Remark,
					command.PlayerRemark,
					cancellationToken).ConfigureAwait(false);
				break;
		}

		await paymentClient.EmitWithdrawFormStatusChangedEventAsync(
			formData.Id,
			formData.BrandId,
			formStatus.ToString(),
			cancellationToken).ConfigureAwait(false);
	}
}
