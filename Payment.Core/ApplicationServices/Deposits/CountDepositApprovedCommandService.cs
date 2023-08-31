using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class CountDepositApprovedCommandService : ICommandService<CountDepositApprovedCommand>
{
	private readonly ICountRepository _countRepository;
	private readonly IPaymentRepository _paymentRepository;

	public CountDepositApprovedCommandService(
		IPaymentRepository paymentRepository,
		ICountRepository countRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_countRepository = countRepository ?? throw new ArgumentNullException(nameof(countRepository));
	}

	public async ValueTask ExecuteAsync(CountDepositApprovedCommand command, CancellationToken cancellationToken = default)
	{
		if (command.Status != PaymentFlowState.Approved.ToString())
			return;

		var depositForm = await _paymentRepository
			.GetDepositFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		var amount = depositForm?.ActualAmount ?? depositForm?.Amount ?? 0m;

		await _countRepository.CountAmountAsync(command.BrandId, (double)amount);
	}
}
