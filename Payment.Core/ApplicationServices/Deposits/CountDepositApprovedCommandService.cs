using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class CountDepositApprovedCommandService : ICommandService<CountDepositApprovedCommand>
{
	private readonly IBrandNetDepositRepository _countRepository;
	private readonly IPaymentRepository _paymentRepository;

	public CountDepositApprovedCommandService(
		IPaymentRepository paymentRepository,
		IBrandNetDepositRepository countRepository)
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

		await _countRepository.CountBrandNetDepositAsync(command.BrandId, (double)amount);
	}
}
