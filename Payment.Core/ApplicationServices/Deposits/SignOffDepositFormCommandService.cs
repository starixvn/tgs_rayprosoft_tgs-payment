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

internal class SignOffDepositFormCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient,
	TimeProvider timeProvider,
	ISerialNumberGenerator serialNumberGenerator,
	ILogger<SignOffDepositFormCommandService> logger) : ICommandService<SignOffDepositFormCommand>
{
	public async ValueTask ExecuteAsync(SignOffDepositFormCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await paymentRepository
			.GetCurrentDepositFormSnapshotAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData == null)
			return;

		var formStatus = formData.Status;

		if (formStatus is PaymentFlowState.Approved or PaymentFlowState.Reject)
			return;

		var eventId = await serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		var currentTime = timeProvider.GetUtcNow().UtcDateTime;

		var updatedForm = formData with
		{
			Status = command.ReviewState switch
			{
				ReviewState.Approve => PaymentFlowState.Approved,
				ReviewState.Reject => PaymentFlowState.Reject,
				_ => throw new NotSupportedException()
			},
			ReviewedUserId = command.ReviewUserId ?? formData.ReviewedUserId,
			ReviewedUserName = command.ReviewUserName ?? formData.ReviewedUserName,
			ReviewedOn = currentTime,
			Remark = command.Remark
		};

		var changeSets = formData.CreatePatch(updatedForm, PaymentUtility.SerializerOptions);

		await paymentRepository.CreateDepositFormEventAsync(
			eventId,
			command.FormId,
			EventAction.Review,
			JsonSerializer.SerializeToDocument(changeSets),
			command.ReviewUserId,
			cancellationToken).ConfigureAwait(false);

		await paymentRepository.UpdateDepositFormSnapshotAsync(
			updatedForm,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		logger.LogInformation("Sign off deposit form send EmitDepositFormStatusChangedEvent, brandId:{brandId}, formId:{formId}, status:{status}",
			formData.BrandId,
			command.FormId,
			formStatus.ToString());

		await paymentClient
			.EmitDepositFormStatusChangedEventAsync(
				formData.Id,
				formData.BrandId,
				formStatus.ToString(),
				cancellationToken)
			.ConfigureAwait(false);
	}
}
