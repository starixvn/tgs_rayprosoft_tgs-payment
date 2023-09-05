using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class CountDepositApprovedCommandService : ICommandService<CountDepositApprovedCommand>
{
	private readonly IBrandNetDepositRepository _countRepository;
	private readonly ILogger<CountDepositApprovedCommandService> _logger;
	private readonly IPaymentRepository _paymentRepository;

	public CountDepositApprovedCommandService(
		IPaymentRepository paymentRepository,
		IBrandNetDepositRepository countRepository,
		ILogger<CountDepositApprovedCommandService> logger)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_countRepository = countRepository ?? throw new ArgumentNullException(nameof(countRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask ExecuteAsync(CountDepositApprovedCommand command, CancellationToken cancellationToken = default)
	{
		if (command.Status != PaymentFlowState.WaitApprove)
			return;

		var depositForm = await _paymentRepository
			.GetDepositFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		_logger.LogInformation("command Status:{commandStatus}, depositForm status:{formStatus}",
			command.Status,
			depositForm!.Status);

		if (depositForm!.Status == PaymentFlowState.Reject)
			return;		

		var amount = depositForm?.ActualAmount ?? depositForm?.Amount ?? 0m;

		await _countRepository.CountBrandCurrentDayNetDepositAsync(command.BrandId, (double)amount);
	}
}
