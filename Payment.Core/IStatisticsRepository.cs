using System;
using System.Collections.Generic;
using System.Threading;

namespace Tgs.Payment;

public interface IStatisticsRepository
{
	IAsyncEnumerable<UserPaymentSummaryStatistics> Query5minPaymentStatisticsAsync(
		DateTime start,
		DateTime end,
		string? userId,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<UserPaymentSummaryStatistics> QueryHourPaymentStatisticsAsync(
		DateTime start,
		DateTime end,
		string? userId,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<UserPaymentSummaryStatistics> QueryDailyPaymentStatisticsAsync(
		DateOnly start,
		DateOnly end,
		string? userId,
		CancellationToken cancellationToken = default);
}
