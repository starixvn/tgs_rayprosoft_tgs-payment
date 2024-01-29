using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Statistics;

internal class PaymentStatisticsQueryService(
	TimeProvider timeProvider,
	IStatisticsRepository statisticsRepository)
	: IQueryService<PaymentStatisticsQuery, UserPaymentSummaryStatistics>
{
	public async IAsyncEnumerable<UserPaymentSummaryStatistics> QueryAsync(
		PaymentStatisticsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var dailyData = await statisticsRepository.QueryDailyPaymentStatisticsAsync(
			query.Start,
			query.End,
			query.UserIds,
			cancellationToken)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		var maxQueryDate = dailyData.Length > 0
			? (DateOnly?)DateOnly.FromDateTime(
				new DateTimeOffset(dailyData.Max(s => s.TimePoint)).ToOffset(PaymentUtility.ReportOffset).DateTime)
				.AddDays(1)
			: null;
		var currentTime = timeProvider.GetUtcNow().ToOffset(PaymentUtility.ReportOffset);
		var currentDate = DateOnly.FromDateTime(currentTime.DateTime);
		var lastRowDate = currentDate.AddDays(-1);
		if (maxQueryDate is not null && lastRowDate < maxQueryDate)
			lastRowDate = maxQueryDate.Value;

		if (query.Start <= currentDate && query.End > lastRowDate)
		{
			var startTime = new DateTimeOffset(
				lastRowDate,
				TimeOnly.MinValue,
				PaymentUtility.ReportOffset).UtcDateTime;
			var endTime = currentTime.UtcDateTime;

			var fiveMinutesQuery = from s in statisticsRepository.Query5minPaymentStatisticsAsync(
									currentTime.FloorHourTime().UtcDateTime,
									endTime,
									query.UserIds,
									cancellationToken)
								   let hourPoint = new DateTimeOffset(s.TimePoint).ToOffset(PaymentUtility.ReportOffset).DateTime.FloorHourTime()
								   group s by new { TimePoint = hourPoint, s.UserId } into g
								   let summary = g.ToEnumerable().Aggregate(
									new PaymentAggregate(),
									(accumulate, statistics) =>
									{
										accumulate.DepositAmount = Add(accumulate.DepositAmount, statistics.DepositAmount);
										accumulate.DepositCount = Add(accumulate.DepositCount, statistics.DepositCount);
										accumulate.DepositHandlingFee = Add(accumulate.DepositHandlingFee, statistics.DepositHandlingFee);
										accumulate.FirstDepositOn ??= statistics.FirstDepositOn;
										accumulate.FirstDepositAmount ??= statistics.FirstDepositAmount;
										accumulate.FirstDepositHandlingFee ??= statistics.FirstDepositHandlingFee;
										accumulate.WithdrawAmount = Add(accumulate.WithdrawAmount, statistics.WithdrawAmount);
										accumulate.WithdrawCount = Add(accumulate.WithdrawCount, statistics.WithdrawCount);
										accumulate.WithdrawHandlingFee = Add(accumulate.WithdrawHandlingFee, statistics.WithdrawHandlingFee);
										accumulate.FirstWithdrawOn ??= statistics.FirstWithdrawOn;
										accumulate.FirstWithdrawAmount ??= statistics.FirstWithdrawAmount;
										accumulate.FirstWithdrawHandlingFee ??= statistics.FirstWithdrawHandlingFee;

										return accumulate;
									})
								   select new UserPaymentSummaryStatistics(
									g.Key.TimePoint,
									g.Key.UserId,
									UserType.Player,
									summary.DepositAmount,
									summary.DepositHandlingFee,
									summary.DepositCount,
									summary.FirstDepositAmount,
									summary.FirstDepositHandlingFee,
									summary.FirstDepositOn,
									summary.WithdrawAmount,
									summary.WithdrawHandlingFee,
									summary.WithdrawCount,
									summary.FirstWithdrawAmount,
									summary.FirstWithdrawHandlingFee,
									summary.FirstWithdrawOn);

			var fiveMinutesData = await fiveMinutesQuery.ToArrayAsync(cancellationToken).ConfigureAwait(false);

			var hourQuery = from s in statisticsRepository.QueryHourPaymentStatisticsAsync(
								startTime,
								endTime,
								query.UserIds,
								cancellationToken).Concat(fiveMinutesData.ToAsyncEnumerable())
							let date = DateOnly.FromDateTime(
								new DateTimeOffset(s.TimePoint).ToOffset(PaymentUtility.ReportOffset).DateTime)
							group s by new { Date = date, s.UserId } into g
							let summary = g.ToEnumerable().Aggregate(
								new PaymentAggregate(),
								(accumulate, statistics) =>
								{
									accumulate.DepositAmount = Add(accumulate.DepositAmount, statistics.DepositAmount);
									accumulate.DepositCount = Add(accumulate.DepositCount, statistics.DepositCount);
									accumulate.DepositHandlingFee = Add(accumulate.DepositHandlingFee, statistics.DepositHandlingFee);
									accumulate.FirstDepositOn ??= statistics.FirstDepositOn;
									accumulate.FirstDepositAmount ??= statistics.FirstDepositAmount;
									accumulate.FirstDepositHandlingFee ??= statistics.FirstDepositHandlingFee;
									accumulate.WithdrawAmount = Add(accumulate.WithdrawAmount, statistics.WithdrawAmount);
									accumulate.WithdrawCount = Add(accumulate.WithdrawCount, statistics.WithdrawCount);
									accumulate.WithdrawHandlingFee = Add(accumulate.WithdrawHandlingFee, statistics.WithdrawHandlingFee);
									accumulate.FirstWithdrawOn ??= statistics.FirstWithdrawOn;
									accumulate.FirstWithdrawAmount ??= statistics.FirstWithdrawAmount;
									accumulate.FirstWithdrawHandlingFee ??= statistics.FirstWithdrawHandlingFee;

									return accumulate;
								})
							select new UserPaymentSummaryStatistics(
								new DateTimeOffset(
									g.Key.Date,
									TimeOnly.MinValue,
									PaymentUtility.ReportOffset).UtcDateTime,
								g.Key.UserId,
								UserType.Player,
								summary.DepositAmount,
								summary.DepositHandlingFee,
								summary.DepositCount,
								summary.FirstDepositAmount,
								summary.FirstDepositHandlingFee,
								summary.FirstDepositOn,
								summary.WithdrawAmount,
								summary.WithdrawHandlingFee,
								summary.WithdrawCount,
								summary.FirstWithdrawAmount,
								summary.FirstWithdrawHandlingFee,
								summary.FirstWithdrawOn);

			var hourData = await hourQuery.ToArrayAsync(cancellationToken).ConfigureAwait(false);

			foreach (var item in dailyData.Concat(hourData)
				.OrderBy(s => s.TimePoint))
				yield return item;

			yield break;
		}

		foreach (var item in dailyData)
			yield return item;
	}

	private static decimal? Add(decimal? summand, decimal? addend)
		=> summand is null
			? addend
			: addend is null
				? summand
				: summand.Value + addend.Value;

	private static int? Add(int? summand, int? addend)
		=> summand is null
			? addend
			: addend is null
				? summand
				: summand.Value + addend.Value;
}
