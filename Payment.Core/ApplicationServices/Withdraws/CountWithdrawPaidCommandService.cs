using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class CountWithdrawPaidCommandService : ICommandService<CountWithdrawPaidCommand>
{
	private readonly IBrandNetDepositRepository _countRepository;
	private readonly IPaymentRepository _paymentRepository;

	public CountWithdrawPaidCommandService(
		IPaymentRepository paymentRepository,
		IBrandNetDepositRepository countRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_countRepository = countRepository ?? throw new ArgumentNullException(nameof(countRepository));
	}

	public async ValueTask ExecuteAsync(CountWithdrawPaidCommand command, CancellationToken cancellationToken = default)
	{
		if (command.Status != PaymentFlowState.Paid.ToString())
			return;

		var withdrawForm = await _paymentRepository
			.GetWithdrawFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (Enum.Parse<PaymentFlowState>(command.Status) == withdrawForm!.Status)
			return;

		var amount = withdrawForm?.ActualAmount ?? withdrawForm?.Amount ?? 0m;

		await _countRepository.CountBrandNetDepositAsync(command.BrandId, (double)-amount);
	}
}
