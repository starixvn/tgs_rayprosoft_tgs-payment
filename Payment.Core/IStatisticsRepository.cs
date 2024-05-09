using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tgs.Payment;

public interface IStatisticsRepository
{
	IAsyncEnumerable<UserPaymentSummaryStatistics> Query5minPaymentStatisticsAsync(
		DateTime start,
		DateTime end,
		string[]? userIds,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<UserPaymentStatisticsDelta> Query5minPaymentStatisticsDeltaAsync(
		Guid anchorId,
		Guid stopId,
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

	ValueTask<decimal> QueryBrandNetDepositAsync(
		DateTime start,
		DateTime end,
		Guid brandId,
		CancellationToken cancellationToken = default);
}