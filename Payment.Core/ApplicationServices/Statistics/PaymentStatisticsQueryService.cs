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
	private static readonly TimeSpan _ReportOffset = TimeSpan.FromHours(8D);

	public async IAsyncEnumerable<UserPaymentSummaryStatistics> QueryAsync(
		PaymentStatisticsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var dailyData = await statisticsRepository.QueryDailyPaymentStatisticsAsync(
			query.Start,
			query.End,
			query.UserId,
			cancellationToken)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		var maxQueryDate = dailyData.Length > 0
			? (DateOnly?)DateOnly.FromDateTime(
				new DateTimeOffset(dailyData.Max(s => s.TimePoint)).ToOffset(_ReportOffset).DateTime)
				.AddDays(1)
			: null;
		var currentTime = timeProvider.GetUtcNow().ToOffset(_ReportOffset);
		var currentDate = DateOnly.FromDateTime(currentTime.DateTime);
		var lastRowDate = currentDate.AddDays(-1);
		if (maxQueryDate is not null && lastRowDate < maxQueryDate)
			lastRowDate = maxQueryDate.Value;

		if (query.Start <= currentDate && query.End > lastRowDate)
		{
			var startTime = new DateTimeOffset(
				lastRowDate,
				TimeOnly.MinValue,
				_ReportOffset).UtcDateTime;
			var endTime = currentTime.UtcDateTime;

			var fiveMinutesQuery = from s in statisticsRepository.Query5minPaymentStatisticsAsync(
									currentTime.FloorHourTime().UtcDateTime,
									endTime,
									query.UserId,
									cancellationToken)
								   let hourPoint = new DateTimeOffset(s.TimePoint).ToOffset(_ReportOffset).DateTime.FloorHourTime()
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
								query.UserId,
								cancellationToken).Concat(fiveMinutesData.ToAsyncEnumerable())
							let date = DateOnly.FromDateTime(
								new DateTimeOffset(s.TimePoint).ToOffset(_ReportOffset).DateTime)
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
									_ReportOffset).UtcDateTime,
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

	private class PaymentAggregate
	{
		public decimal? DepositAmount { get; set; }

		public decimal? DepositHandlingFee { get; set; }

		public int? DepositCount { get; set; }

		public decimal? FirstDepositAmount { get; set; }

		public decimal? FirstDepositHandlingFee { get; set; }

		public DateTime? FirstDepositOn { get; set; }

		public decimal? WithdrawAmount { get; set; }

		public decimal? WithdrawHandlingFee { get; set; }

		public int? WithdrawCount { get; set; }

		public decimal? FirstWithdrawAmount { get; set; }

		public decimal? FirstWithdrawHandlingFee { get; set; }

		public DateTime? FirstWithdrawOn { get; set; }
	}
}
