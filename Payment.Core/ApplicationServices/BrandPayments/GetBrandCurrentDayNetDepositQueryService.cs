using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandCurrentDayNetDepositQueryService(IStatisticsRepository repository)
	: IGetService<GetBrandCurrentDayNetDepositQuery, decimal>
{
	public async ValueTask<decimal> GetAsync(GetBrandCurrentDayNetDepositQuery query, CancellationToken cancellationToken = default)
	{
		var start = new DateTimeOffset(DateTimeOffset.UtcNow.ToOffset(PaymentUtility.ReportOffset).Date, PaymentUtility.ReportOffset);

		var result = await repository
			.QueryBrandNetDepositAsync(start.UtcDateTime, DateTime.UtcNow, query.BrandId, cancellationToken)
			.ConfigureAwait(false);

		return result;
	}
}