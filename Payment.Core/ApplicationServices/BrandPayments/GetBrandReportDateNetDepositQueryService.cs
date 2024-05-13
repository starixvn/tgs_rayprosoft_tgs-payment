using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandReportDateNetDepositQueryService(
	TimeProvider timeProvider,
	IPaymentRepository paymentRepository,
	IStatisticsRepository  repository) : IGetService<GetBrandReportDateNetDepositQuery, decimal>
{
	public async ValueTask<decimal> GetAsync(GetBrandReportDateNetDepositQuery query, CancellationToken cancellationToken = default)
	{
		var now = timeProvider.GetUtcNow().ToOffset(TimeSpan.FromHours(8D));
		var currentDate = DateOnly.FromDateTime(now.DateTime);
		var yesterday = currentDate.AddDays(-1);

		var scheduleTime = new DateTimeOffset(now.Year, now.Month, now.Day, 8, 0, 0, TimeSpan.FromHours(8));

		if (query.ReportDate == currentDate)
		{
			var start = new DateTimeOffset(currentDate.ToDateTime(TimeOnly.MinValue), PaymentUtility.ReportOffset);

			var result = await repository
				.QueryBrandNetDepositAsync(start.UtcDateTime, timeProvider.GetUtcNow().DateTime, query.BrandId, cancellationToken)
				.ConfigureAwait(false);

			return result;
		}

		if (query.ReportDate == yesterday && now < scheduleTime)
		{
			var start = new DateTimeOffset(yesterday.ToDateTime(TimeOnly.MinValue), PaymentUtility.ReportOffset);

			var end = start.AddDays(1);

			var result = await repository
				.QueryBrandNetDepositAsync(start.UtcDateTime, end.UtcDateTime, query.BrandId, cancellationToken)
				.ConfigureAwait(false);

			return result;
		}

		var netDeposit = await paymentRepository
			.GetBrandNetDepositAsync(query.BrandId, query.ReportDate, cancellationToken)
			.ConfigureAwait(false);

		return netDeposit == null
			? 0m
			: netDeposit.Value;
	}
}
