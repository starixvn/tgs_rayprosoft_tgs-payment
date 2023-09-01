using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class CountWithdrawPaidCommandService : ICommandService<CountWithdrawPaidCommand>
{
	private readonly IBrandNetDepositRepository _countRepository;
	private readonly ILogger<CountWithdrawPaidCommandService> _logger;
	private readonly IPaymentRepository _paymentRepository;

	public CountWithdrawPaidCommandService(
		IPaymentRepository paymentRepository,
		IBrandNetDepositRepository countRepository,
		ILogger<CountWithdrawPaidCommandService> logger)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_countRepository = countRepository ?? throw new ArgumentNullException(nameof(countRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask ExecuteAsync(CountWithdrawPaidCommand command, CancellationToken cancellationToken = default)
	{
		if (command.Status != PaymentFlowState.Approved)
			return;

		var withdrawForm = await _paymentRepository
			.GetWithdrawFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		_logger.LogInformation("command Status:{commandStatus}, withdrawForm status:{formStatus}",
			command.Status,
			withdrawForm!.Status);

		if (command.Status == withdrawForm!.Status || withdrawForm!.Status == PaymentFlowState.Reject)
			return;

		var amount = withdrawForm?.ActualAmount ?? withdrawForm?.Amount ?? 0m;

		await _countRepository.CountBrandNetDepositAsync(command.BrandId, (double)-amount);
	}
}
