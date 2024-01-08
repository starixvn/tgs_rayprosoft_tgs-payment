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

internal class SubmitWithdrawFormStatusCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient,
	TimeProvider timeProvider,
	ISerialNumberGenerator serialNumberGenerator,
	ILogger<SubmitWithdrawFormStatusCommandService> logger)
	: ICommandService<SubmitWithdrawFormStatusCommand>
{
	public async ValueTask ExecuteAsync(SubmitWithdrawFormStatusCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await paymentRepository
			.GetCurrentWithdrawFormSnapshotAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData is null)
			return;

		var originStatus = formData.Status;

		logger.LogInformation("SubmitWithdrawFormStatusCommand form origin Status:{originStatus}, command:{command}",
			originStatus,
			JsonSerializer.Serialize(command, PaymentUtility.SerializerOptions));

		var currentTime = timeProvider.GetUtcNow().UtcDateTime;
		var newFormData = formData with
		{
			Status = command.Status ?? formData.Status,
			PaymentReturn = command.PaymentReturn ?? formData.PaymentReturn,
			ActualAmount = command.ActualAmount ?? formData.ActualAmount,
			ThirdPartyOrderNo = command.ThirdPartyOrderNo ?? formData.ThirdPartyOrderNo,
			PaidUserId = command.PaidUserId ?? formData.PaidUserId,
			PaidUserName = command.PaidUserName ?? formData.PaidUserName,
			PaidOn = command.Status == PaymentFlowState.Paid ? currentTime : formData.PaidOn
		};

		var changeSets = formData.CreatePatch(newFormData, PaymentUtility.SerializerOptions);
		var eventId = await serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		await paymentRepository.CreateWithdrawFormEventAsync(
			eventId,
			command.FormId,
			EventAction.ChangeStatus,
			JsonSerializer.SerializeToDocument(changeSets),
			null,
			cancellationToken).ConfigureAwait(false);

		await paymentRepository.UpdateWithdrawFormSnapshotAsync(
			newFormData,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		if (command.Status is not null && originStatus != command.Status)
		{
			await paymentClient
				.EmitWithdrawFormStatusChangedEventAsync(
					command.FormId,
					formData.BrandId,
					originStatus.ToString(),
					cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
