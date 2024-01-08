using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class SignOffWithdrawFormCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient,
	TimeProvider timeProvider,
	ISerialNumberGenerator serialNumberGenerator,
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

		var fsm = StateMachine.CreateWithdrawStateMachine(formData.Status);

		var formStatus = formData.Status;

		logger.LogDebug("SignOffWithdrawFormCommand command:{command}, formData Status:{formStatus}",
			JsonSerializer.Serialize(command, PaymentUtility.SerializerOptions),
			formStatus);

		if (command.Status == ReviewState.Approve)
		{
			if (!fsm.CanFire(PaymentTrigger.Approve))
				return;

			await fsm.FireAsync(PaymentTrigger.Approve).ConfigureAwait(false);

			await withdrawService.ApproveWithdrawFormAsync(
				command.BrandPaymentId!.Value,
				formData,
				fsm.State,
				command.ReviewUserId,
				command.ReviewUserName,
				command.Remark,
				cancellationToken).ConfigureAwait(false);
		}
		else
		{
			if (!fsm.CanFire(PaymentTrigger.Reject))
				return;

			await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

			var currentTime = timeProvider.GetUtcNow().UtcDateTime;
			var updateFormData = formData with
			{
				Status = fsm.State,
				ReviewedUserId = command.ReviewUserId,
				ReviewedUserName = command.ReviewUserName,
				ReviewedOn = currentTime,
				Remark = command.Remark
			};

			var changeSets = formData.CreatePatch(updateFormData, PaymentUtility.SerializerOptions);
			var eventId = await serialNumberGenerator
				.GenerateAsync(cancellationToken)
				.ConfigureAwait(false);

			await paymentRepository.CreateWithdrawFormEventAsync(
				eventId,
				command.FormId,
				EventAction.Review,
				JsonSerializer.SerializeToDocument(changeSets),
				command.ReviewUserId,
				cancellationToken).ConfigureAwait(false);

			await paymentRepository.UpdateWithdrawFormSnapshotAsync(
				updateFormData,
				eventId,
				currentTime,
				cancellationToken).ConfigureAwait(false);
		}

		await paymentClient.EmitWithdrawFormStatusChangedEventAsync(
			formData.Id,
			formData.BrandId,
			formStatus.ToString(),
			cancellationToken).ConfigureAwait(false);
	}
}
