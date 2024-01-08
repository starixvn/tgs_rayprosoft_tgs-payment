using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class ReportDateSuccessRateQueryService(
	ISystemClock systemClock,
	IPaymentRepository paymentRepository) : IGetService<ReportDateSuccessRateQuery, decimal>
{
	public async ValueTask<decimal> GetAsync(ReportDateSuccessRateQuery query, CancellationToken cancellationToken = default)
	{
		var now = systemClock.UtcNow.ToOffset(TimeSpan.FromHours(8D));
		var currentDate = DateOnly.FromDateTime(now.DateTime);
		var yesterday = currentDate.AddDays(-1);

		var scheduleTime = new DateTimeOffset(now.Year, now.Month, now.Day, 8, 0, 0, TimeSpan.FromHours(8));

		if (query.ReportDate == currentDate)
		{
			var currentDayData = await paymentRepository
				.CountCurrentDayDepositFormsStatusAsync(query.BrandId, cancellationToken)
				.ConfigureAwait(false);

			return currentDayData.Total is 0
				? 0m
				: Math.Round((decimal)currentDayData.Approved / currentDayData.Total * 100, MidpointRounding.AwayFromZero);
		}

		if (query.ReportDate == yesterday && now < scheduleTime)
		{
			var yesterDayData = await paymentRepository
				.CountYesterDayDepositFormsStatusAsync(query.BrandId, cancellationToken)
				.ConfigureAwait(false);

			return yesterDayData.Total is 0
				? 0m
				: Math.Round((decimal)yesterDayData.Approved / yesterDayData.Total * 100, MidpointRounding.AwayFromZero);
		}

		var reportData = await paymentRepository
			.GetReportDateDepositFormSummaryAsync(query.BrandId, query.ReportDate, cancellationToken)
			.ConfigureAwait(false);

		return reportData.Total is 0
			? 0m
			: Math.Round((decimal)reportData.Approved / reportData.Total * 100, MidpointRounding.AwayFromZero);
	}
}
