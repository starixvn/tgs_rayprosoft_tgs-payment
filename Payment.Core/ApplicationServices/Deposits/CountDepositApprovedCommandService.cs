using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class CountDepositApprovedCommandService(
	IPaymentRepository paymentRepository,
	IBrandNetDepositRepository countRepository,
	ILogger<CountDepositApprovedCommandService> logger) : ICommandService<CountDepositApprovedCommand>
{
	public async ValueTask ExecuteAsync(CountDepositApprovedCommand command, CancellationToken cancellationToken = default)
	{
		if (command.Status != PaymentFlowState.WaitApprove)
			return;

		var depositForm = await paymentRepository
			.GetDepositFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("command Status:{commandStatus}, depositForm status:{formStatus}",
			command.Status,
			depositForm!.Status);

		if (depositForm!.Status == PaymentFlowState.Reject)
			return;

		var amount = depositForm?.ActualAmount ?? depositForm?.Amount ?? 0m;

		await countRepository.CountBrandCurrentDayNetDepositAsync(
			command.BrandId,
			(double)amount).ConfigureAwait(false);
	}
}
