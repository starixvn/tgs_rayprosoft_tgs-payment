using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class CurrentDaySuccessRateQueryService(IPaymentRepository paymentRepository) : IGetService<CurrentDaySuccessRateQuery, decimal>
{
	public async ValueTask<decimal> GetAsync(CurrentDaySuccessRateQuery query, CancellationToken cancellationToken = default)
	{
		var result = await paymentRepository
			.CountCurrentDayDepositFormsStatusAsync(query.BrandId, cancellationToken)
			.ConfigureAwait(false);

		return result.Total is 0
			? 0m
			: Math.Round((decimal)result.Approved / result.Total * 100, MidpointRounding.AwayFromZero);
	}
}
