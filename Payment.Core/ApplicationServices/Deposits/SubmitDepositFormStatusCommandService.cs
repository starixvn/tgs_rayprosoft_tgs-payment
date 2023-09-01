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

internal class SubmitDepositFormStatusCommandService : ICommandService<SubmitDepositFormStatusCommand>
{
	private readonly ILogger<SubmitDepositFormStatusCommandService> _logger;
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly ISystemClock _systemClock;

	public SubmitDepositFormStatusCommandService(
		IPaymentRepository paymentRepository,
		IPaymentClient paymentClient,
		ISystemClock systemClock,
		ISerialNumberGenerator serialNumberGenerator,
		ILogger<SubmitDepositFormStatusCommandService> logger)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask ExecuteAsync(SubmitDepositFormStatusCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await _paymentRepository
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
		var eventId = await _serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);
		var currentTime = _systemClock.UtcNow.UtcDateTime;

		await _paymentRepository.CreateDepositFormEventAsync(
			eventId,
			command.FormId,
			EventAction.ChangeStatus,
			JsonSerializer.SerializeToDocument(changeSets),
			null,
			cancellationToken).ConfigureAwait(false);

		await _paymentRepository.UpdateDepositFormSnapshotAsync(
			newFormData,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		if (command.Status is not null && originStatus != command.Status)
		{
			_logger.LogInformation("Submit deposit form status send EmitDepositFormStatusChangedEvent, originStatus:{originStatus}, command status:{status}, formId:{formId}",
				originStatus,
				command.Status,
				command.FormId);

			await _paymentClient.EmitDepositFormStatusChangedEventAsync(
				command.FormId,
				formData.BrandId,
				originStatus.ToString(),
				cancellationToken).ConfigureAwait(false);
		}
	}
}
