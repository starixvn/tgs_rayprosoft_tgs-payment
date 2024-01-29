using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Players;

internal class GetPaymentOverviewQueryService(
	TimeProvider timeProvider,
	IPaymentRepository paymentRepository,
	IStatisticsRepository statisticsRepository)
	: IGetService<GetPaymentOverviewQuery, PaymentOverview?>
{
	public async ValueTask<PaymentOverview?> GetAsync(GetPaymentOverviewQuery query, CancellationToken cancellationToken = default)
	{
		var currentTime = timeProvider.GetUtcNow().ToOffset(PaymentUtility.ReportOffset);

		var overview = await paymentRepository.GetPlayerPaymentOverviewAsync(
			query.PlayerCode,
			cancellationToken).ConfigureAwait(false);

		var currentDate = DateOnly.FromDateTime(currentTime.DateTime);
		var start = currentTime.Hour >= 8
			? new DateTimeOffset(currentDate, TimeOnly.MinValue, PaymentUtility.ReportOffset).UtcDateTime
			: new DateTimeOffset(currentDate.AddDays(-1), TimeOnly.MinValue, PaymentUtility.ReportOffset).UtcDateTime;

		if (overview is not null)
		{
			var recordedOn = new DateTimeOffset(overview.RecordedBy, TimeOnly.MinValue, PaymentUtility.ReportOffset).UtcDateTime;

			if (recordedOn > start)
				start = recordedOn;
		}

		var queryUserIds = new[] { query.PlayerCode };
		var fiveMinutesData = await statisticsRepository.Query5minPaymentStatisticsAsync(
			currentTime.FloorHourTime().UtcDateTime,
			currentTime.UtcDateTime,
			queryUserIds,
			cancellationToken)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		var hourQuery = from s in statisticsRepository.QueryHourPaymentStatisticsAsync(
							start,
							currentTime.FloorHourTime().UtcDateTime,
							queryUserIds,
							cancellationToken).Concat(fiveMinutesData.ToAsyncEnumerable())
						group s by s.UserId into g
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
							currentTime.UtcDateTime,
							g.Key,
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

		var realTimeData = await hourQuery.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

		return (overview ?? PaymentOverview.Default with { RecordedBy = currentDate }) with
		{
			TotalDepositAmount = (overview?.TotalDepositAmount ?? 0M) + (realTimeData?.DepositAmount ?? 0M),
			TotalWithdrawAmount = (overview?.TotalWithdrawAmount ?? 0M) + (realTimeData?.WithdrawAmount ?? 0M),
			FirstDepositAmount = overview?.FirstDepositAmount ?? realTimeData?.FirstDepositAmount,
			FirstDepositHandlingFee = overview?.FirstDepositHandlingFee ?? realTimeData?.FirstDepositHandlingFee,
			FirstDepositOn = overview?.FirstDepositOn ?? realTimeData?.FirstDepositOn,
			FirstWithdrawAmount = overview?.FirstWithdrawAmount ?? realTimeData?.FirstWithdrawAmount,
			FirstWithdrawHandlingFee = overview?.FirstWithdrawHandlingFee ?? realTimeData?.FirstWithdrawHandlingFee,
			FirstWithdrawOn = overview?.FirstWithdrawOn ?? realTimeData?.FirstWithdrawOn
		};
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
