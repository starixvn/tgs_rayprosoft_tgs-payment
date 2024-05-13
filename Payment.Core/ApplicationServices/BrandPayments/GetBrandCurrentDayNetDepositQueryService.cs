using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandCurrentDayNetDepositQueryService(
	TimeProvider timeProvider,
	IStatisticsRepository repository)
	: IGetService<GetBrandCurrentDayNetDepositQuery, decimal>
{
	public async ValueTask<decimal> GetAsync(GetBrandCurrentDayNetDepositQuery query, CancellationToken cancellationToken = default)
	{
		var start = new DateTimeOffset(timeProvider.GetUtcNow().ToOffset(PaymentUtility.ReportOffset).Date, PaymentUtility.ReportOffset);

		var result = await repository
			.QueryBrandNetDepositAsync(start.UtcDateTime, timeProvider.GetUtcNow().DateTime, query.BrandId, cancellationToken)
			.ConfigureAwait(false);

		return result;
	}
}