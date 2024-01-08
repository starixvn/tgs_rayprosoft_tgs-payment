using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class CountWithdrawPaidCommandService(
	IPaymentRepository paymentRepository,
	IBrandNetDepositRepository countRepository,
	ILogger<CountWithdrawPaidCommandService> logger)
	: ICommandService<CountWithdrawPaidCommand>
{
	public async ValueTask ExecuteAsync(CountWithdrawPaidCommand command, CancellationToken cancellationToken = default)
	{
		if (command.Status != PaymentFlowState.Approved)
			return;

		var withdrawForm = await paymentRepository
			.GetWithdrawFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("command Status:{commandStatus}, withdrawForm status:{formStatus}",
			command.Status,
			withdrawForm!.Status);

		if (command.Status == withdrawForm!.Status || withdrawForm!.Status == PaymentFlowState.Reject)
			return;

		var amount = withdrawForm?.ActualAmount ?? withdrawForm?.Amount ?? 0m;

		await countRepository.CountBrandCurrentDayNetDepositAsync(command.BrandId, (double)-amount);
	}
}
