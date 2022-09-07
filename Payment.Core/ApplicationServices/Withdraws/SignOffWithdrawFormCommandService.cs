using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.ApplicationServices.BrandPayments;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class SignOffWithdrawFormCommandService : ICommandService<SignOffWithdrawFormCommand>
{
	private readonly IGetService<CalculateHandlingFeeCommand, decimal> _calculateService;
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;
	private readonly IWithdrawProcessorFactory _withdrawProcessorFactory;

	public SignOffWithdrawFormCommandService(
		IPaymentRepository paymentRepository,
		IWithdrawProcessorFactory withdrawProcessorFactory,
		IPaymentClient paymentClient,
		IGetService<CalculateHandlingFeeCommand, decimal> calculateService)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_withdrawProcessorFactory = withdrawProcessorFactory ?? throw new ArgumentNullException(nameof(withdrawProcessorFactory));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_calculateService = calculateService ?? throw new ArgumentNullException(nameof(calculateService));
	}

	public async ValueTask ExecuteAsync(SignOffWithdrawFormCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await _paymentRepository
			.GetWithdrawFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData == null)
			return;

		var fsm = StateMachine.CreateWithdrawStateMachine(formData.Status);

		var formStatus = formData.Status;

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

			await _paymentRepository
				.SignOffWithdrawFormAsync(
					command.FormId,
					command.BrandPaymentId.Value,
					applyInfo,
					fsm.State,
					command.ReviewUserId,
					command.ReviewUserName,
					command.Remark,
					handlingFee,
					cancellationToken)
				.ConfigureAwait(false);
		}
		else
		{
			if (!fsm.CanFire(PaymentTrigger.Reject))
				return;

			await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

			await _paymentRepository
				.SignOffWithdrawFormAsync(
					command.FormId,
					null,
					null,
					fsm.State,
					command.ReviewUserId,
					command.ReviewUserName,
					command.Remark,
					null,
					cancellationToken)
				.ConfigureAwait(false);
		}

		await _paymentClient
			.EmitWithdrawFormStatusChangedEventAsync(formData.Id, formData.BrandId, formStatus.ToString(), cancellationToken)
			.ConfigureAwait(false);
	}
}
