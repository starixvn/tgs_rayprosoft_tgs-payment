using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class SubmitWithdrawFormStatusCommandService : ICommandService<SubmitWithdrawFormStatusCommand>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly ISystemClock _systemClock;

	public SubmitWithdrawFormStatusCommandService(
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

	public async ValueTask ExecuteAsync(SubmitWithdrawFormStatusCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await _paymentRepository
			.GetCurrentWithdrawFormSnapshotAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData is null)
			return;

		var originStatus = formData.Status;

		var currentTime = _systemClock.UtcNow.UtcDateTime;
		var newFormData = formData with
		{
			Status = command.Status ?? formData.Status,
			PaymentReturn = command.PaymentReturn ?? formData.PaymentReturn,
			ActualAmount = command.ActualAmount ?? formData.ActualAmount,
			ThirdPartyOrderNo = command.ThirdPartyOrderNo ?? formData.ThirdPartyOrderNo,
			PaidUserId = command.PaidUserId ?? formData.PaidUserId,
			PaidUserName = command.PaidUserName ?? formData.PaidUserName,
			PaidOn = command.PaidUserId is null ? null : currentTime
		};

		var changeSets = formData.CreatePatch(newFormData, PaymentUtility.SerializerOptions);
		var eventId = await _serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		await _paymentRepository.CreateWithdrawFormEventAsync(
			eventId,
			command.FormId,
			EventAction.ChangeStatus,
			JsonSerializer.SerializeToDocument(changeSets),
			null,
			cancellationToken).ConfigureAwait(false);

		await _paymentRepository.UpdateWithdrawFormSnapshotAsync(
			newFormData,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		if (originStatus != command.Status)
			await _paymentClient
				.EmitWithdrawFormStatusChangedEventAsync(
					command.FormId,
					formData.BrandId,
					originStatus.ToString(),
					cancellationToken)
				.ConfigureAwait(false);
	}
}
