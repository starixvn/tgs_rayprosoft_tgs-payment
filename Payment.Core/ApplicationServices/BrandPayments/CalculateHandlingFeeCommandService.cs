using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class CalculateHandlingFeeCommandService(IPaymentRepository paymentRepository) : IGetService<CalculateHandlingFeeCommand, decimal>
{
	public async ValueTask<decimal> GetAsync(CalculateHandlingFeeCommand command, CancellationToken cancellationToken = default)
	{
		var brandPaymentFeeInfo = await paymentRepository
			.GetBrandPaymentFeeSettingAsync(command.BrandPaymentId, cancellationToken)
			.ConfigureAwait(false);

		return brandPaymentFeeInfo.ReducePlayerAmount is false
			? 0m
			: command.Amount * brandPaymentFeeInfo.HandlingFeeRateTenPercentile * 0.001m;
	}
}
