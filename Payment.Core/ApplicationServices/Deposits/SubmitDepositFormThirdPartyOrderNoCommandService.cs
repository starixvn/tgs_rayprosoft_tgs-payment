using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class SubmitDepositFormThirdPartyOrderNoCommandService : ICommandService<SubmitDepositFormThirdPartyOrderNoCommand>
{
	private readonly IPaymentRepository _paymentRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly ISystemClock _systemClock;

	public SubmitDepositFormThirdPartyOrderNoCommandService(
		IPaymentRepository paymentRepository,
		ISystemClock systemClock,
		ISerialNumberGenerator serialNumberGenerator)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
	}

	public async ValueTask ExecuteAsync(SubmitDepositFormThirdPartyOrderNoCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await _paymentRepository
			.GetCurrentDepositFormSnapshotAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData is null)
			return;

		var newFormData = formData with
		{
			ThirdPartyOrderNo = command.ThirdPartyOrderNo ?? formData.ThirdPartyOrderNo
		};

		var changeSets = newFormData.CreatePatch(formData, PaymentUtility.SerializerOptions);
		var eventId = await _serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);
		var currentTime = _systemClock.UtcNow.UtcDateTime;

		await _paymentRepository.CreateDepositFormEventAsync(
			eventId,
			command.FormId,
			DepositFormAction.ChangeStatus,
			JsonSerializer.SerializeToDocument(changeSets),
			null,
			cancellationToken).ConfigureAwait(false);

		await _paymentRepository.UpdateDepositFormSnapshotAsync(
			newFormData,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);
	}
}
