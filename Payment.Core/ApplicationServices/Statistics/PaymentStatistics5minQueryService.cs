using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Statistics;

internal class PaymentStatistics5minQueryService(
	IStatisticsRepository statisticsRepository)
	: IQueryService<PaymentStatistics5minQuery, UserPaymentSummaryStatistics>
{
	public IAsyncEnumerable<UserPaymentSummaryStatistics> QueryAsync(PaymentStatistics5minQuery query, CancellationToken cancellationToken = default)
		=> statisticsRepository.Query5minPaymentStatisticsAsync(
			query.Start,
			query.End,
			null,
			cancellationToken);
}
