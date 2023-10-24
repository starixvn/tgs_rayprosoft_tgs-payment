using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ApplicationServices.BrandPayments;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class SignOffWithdrawFormCommandService : ICommandService<SignOffWithdrawFormCommand>
{
	private readonly IGetService<CalculateHandlingFeeCommand, decimal> _calculateService;
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly ILogger<SignOffWithdrawFormCommandService> _logger;
	private readonly ISystemClock _systemClock;
	private readonly IWithdrawProcessorFactory _withdrawProcessorFactory;

	public SignOffWithdrawFormCommandService(
		IPaymentRepository paymentRepository,
		IWithdrawProcessorFactory withdrawProcessorFactory,
		IPaymentClient paymentClient,
		IGetService<CalculateHandlingFeeCommand, decimal> calculateService,
		ISystemClock systemClock,
		ISerialNumberGenerator serialNumberGenerator,
		ILogger<SignOffWithdrawFormCommandService> logger)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_withdrawProcessorFactory = withdrawProcessorFactory ?? throw new ArgumentNullException(nameof(withdrawProcessorFactory));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_calculateService = calculateService ?? throw new ArgumentNullException(nameof(calculateService));
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask ExecuteAsync(SignOffWithdrawFormCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await _paymentRepository
			.GetCurrentWithdrawFormSnapshotAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData is null)
			return;

		var fsm = StateMachine.CreateWithdrawStateMachine(formData.Status);

		var formStatus = formData.Status;

		_logger.LogDebug("SignOffWithdrawFormCommand command:{command}, formData Status:{formStatus}",
			JsonSerializer.Serialize(command, PaymentUtility.SerializerOptions),
			formStatus);

		if (command.Status == ReviewState.Approve)
		{
			if (!fsm.CanFire(PaymentTrigger.Approve))
				return;

			await fsm.FireAsync(PaymentTrigger.Approve).ConfigureAwait(false);

			var brandPayment = await _paymentRepository
				.GetBrandPaymentByIdAsync(command.BrandPaymentId!.Value, cancellationToken)
				.ConfigureAwait(false);

			var payment = await _paymentRepository
				.GetPaymentAsync(brandPayment.PaymentId, cancellationToken)
				.ConfigureAwait(false);

			var processor = _withdrawProcessorFactory.GetWithdrawProcessor(payment.Code);

			var handlingFee = await _calculateService
				.GetAsync(new CalculateHandlingFeeCommand(command.BrandPaymentId.Value, formData.Amount), cancellationToken)
				.ConfigureAwait(false);

			var withdrawSettings = processor.MakeWithdrawSettings(
				command.FormId,
				formData.Amount,
				formData.BankInfo,
				brandPayment);

			var applyInfo = await _paymentClient
				.WithdrawAsync(
					processor.Subject,
					formData.Amount - handlingFee,
					withdrawSettings,
					cancellationToken)
				.ConfigureAwait(false);

			var currentTime = _systemClock.UtcNow.UtcDateTime;
			var updateFormData = formData with
			{
				BrandPaymentId = command.BrandPaymentId.Value,
				ApplyInfo = applyInfo,
				Status = fsm.State,
				ReviewedUserId = command.ReviewUserId,
				ReviewedUserName = command.ReviewUserName,
				ReviewedOn = currentTime,
				Remark = command.Remark,
				HandlingFee = handlingFee
			};

			var changeSets = formData.CreatePatch(updateFormData, PaymentUtility.SerializerOptions);
			var eventId = await _serialNumberGenerator
				.GenerateAsync(cancellationToken)
				.ConfigureAwait(false);

			await _paymentRepository.CreateWithdrawFormEventAsync(
				eventId,
				command.FormId,
				EventAction.Review,
				JsonSerializer.SerializeToDocument(changeSets),
				command.ReviewUserId,
				cancellationToken).ConfigureAwait(false);

			await _paymentRepository.UpdateWithdrawFormSnapshotAsync(
				updateFormData,
				eventId,
				currentTime,
				cancellationToken).ConfigureAwait(false);
		}
		else
		{
			if (!fsm.CanFire(PaymentTrigger.Reject))
				return;

			await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

			var currentTime = _systemClock.UtcNow.UtcDateTime;
			var updateFormData = formData with
			{
				Status = fsm.State,
				ReviewedUserId = command.ReviewUserId,
				ReviewedUserName = command.ReviewUserName,
				ReviewedOn = currentTime,
				Remark = command.Remark
			};

			var changeSets = formData.CreatePatch(updateFormData, PaymentUtility.SerializerOptions);
			var eventId = await _serialNumberGenerator
				.GenerateAsync(cancellationToken)
				.ConfigureAwait(false);

			await _paymentRepository.CreateWithdrawFormEventAsync(
				eventId,
				command.FormId,
				EventAction.Review,
				JsonSerializer.SerializeToDocument(changeSets),
				command.ReviewUserId,
				cancellationToken).ConfigureAwait(false);

			await _paymentRepository.UpdateWithdrawFormSnapshotAsync(
				updateFormData,
				eventId,
				currentTime,
				cancellationToken).ConfigureAwait(false);
		}

		await _paymentClient.EmitWithdrawFormStatusChangedEventAsync(
			formData.Id,
			formData.BrandId,
			formStatus.ToString(),
			cancellationToken).ConfigureAwait(false);
	}
}
