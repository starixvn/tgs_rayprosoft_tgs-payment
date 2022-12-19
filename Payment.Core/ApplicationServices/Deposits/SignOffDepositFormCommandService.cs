using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class SignOffDepositFormCommandService : ICommandService<SignOffDepositFormCommand>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly ISystemClock _systemClock;

	public SignOffDepositFormCommandService(
		IPaymentRepository paymentRepository,
		IPaymentClient paymentClient,
		ISystemClock systemClock,
		ISerialNumberGenerator serialNumberGenerator)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
	}

	public async ValueTask ExecuteAsync(SignOffDepositFormCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await _paymentRepository
			.GetCurrentDepositFormSnapshotAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData == null)
			return;

		var formStatus = formData.Status;

		if (formStatus is PaymentFlowState.Approved or PaymentFlowState.Reject)
			return;

		var eventId = await _serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		var currentTime = _systemClock.UtcNow.UtcDateTime;

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

		await _paymentRepository.CreateDepositFormEventAsync(
			eventId,
			command.FormId,
			DepositFormAction.Review,
			JsonSerializer.SerializeToDocument(changeSets),
			command.ReviewUserId,
			cancellationToken).ConfigureAwait(false);

		await _paymentRepository.UpdateDepositFormSnapshotAsync(
			updatedForm,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		await _paymentClient
			.EmitDepositFormStatusChangedEventAsync(
				formData.Id,
				formData.BrandId,
				formStatus.ToString(),
				cancellationToken)
			.ConfigureAwait(false);
	}
}
