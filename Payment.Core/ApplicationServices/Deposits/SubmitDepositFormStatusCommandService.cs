using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class SubmitDepositFormStatusCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient,
	TimeProvider timeProvider,
	ISerialNumberGenerator serialNumberGenerator,
	ILogger<SubmitDepositFormStatusCommandService> logger) : ICommandService<SubmitDepositFormStatusCommand>
{
	public async ValueTask ExecuteAsync(SubmitDepositFormStatusCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await paymentRepository
			.GetCurrentDepositFormSnapshotAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData == null)
			return;

		var originStatus = formData.Status;

		var newFormData = formData with
		{
			Status = command.Status ?? formData.Status,
			PaymentReturn = command.PaymentReturn ?? formData.PaymentReturn,
			ActualAmount = command.ActualAmount ?? formData.ActualAmount,
			ThirdPartyOrderNo = command.ThirdPartyOrderNo ?? formData.ThirdPartyOrderNo,
			HandlingFee = command.HandlingFee ?? formData.HandlingFee,
			PromotionPlayerId = command.PromotionPlayerId ?? formData.PromotionPlayerId
		};

		var changeSets = formData.CreatePatch(newFormData, PaymentUtility.SerializerOptions);
		var eventId = await serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);
		var currentTime = timeProvider.GetUtcNow().UtcDateTime;

		await paymentRepository.CreateDepositFormEventAsync(
			eventId,
			command.FormId,
			EventAction.ChangeStatus,
			JsonSerializer.SerializeToDocument(changeSets),
			null,
			cancellationToken).ConfigureAwait(false);

		await paymentRepository.UpdateDepositFormSnapshotAsync(
			newFormData,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		if (command.Status is not null && originStatus != command.Status)
		{
			logger.LogInformation(
				"Submit deposit form status send EmitDepositFormStatusChangedEvent, " +
					"originStatus:{originStatus}, command status:{status}, formId:{formId}",
				originStatus,
				command.Status,
				command.FormId);

			await paymentClient.EmitDepositFormStatusChangedEventAsync(
				command.FormId,
				formData.BrandId,
				originStatus.ToString(),
				cancellationToken).ConfigureAwait(false);
		}
	}
}
