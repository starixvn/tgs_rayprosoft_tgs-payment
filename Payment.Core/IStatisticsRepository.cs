using System;
using System.Collections.Generic;
using System.Threading;

namespace Tgs.Payment;

public interface IStatisticsRepository
{
	IAsyncEnumerable<UserPaymentSummaryStatistics> Query5minPaymentStatisticsAsync(
		DateTime start,
		DateTime end,
		string[]? userIds,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<UserPaymentSummaryStatistics> QueryHourPaymentStatisticsAsync(
		DateTime start,
		DateTime end,
		string[]? userIds,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<UserPaymentSummaryStatistics> QueryDailyPaymentStatisticsAsync(
		DateOnly start,
		DateOnly end,
		string[]? userIds,
		CancellationToken cancellationToken = default);
}
