using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandReportDateNetDepositQueryService(
	TimeProvider timeProvider,
	IPaymentRepository paymentRepository,
	IBrandNetDepositRepository brandNetDepositRepository) : IGetService<GetBrandReportDateNetDepositQuery, decimal>
{
	public async ValueTask<decimal> GetAsync(GetBrandReportDateNetDepositQuery query, CancellationToken cancellationToken = default)
	{
		var now = timeProvider.GetUtcNow().ToOffset(TimeSpan.FromHours(8D));
		var currentDate = DateOnly.FromDateTime(now.DateTime);
		var yesterday = currentDate.AddDays(-1);

		var scheduleTime = new DateTimeOffset(now.Year, now.Month, now.Day, 8, 0, 0, TimeSpan.FromHours(8));

		if (query.ReportDate == currentDate)
		{
			return await brandNetDepositRepository
				.GetBrandCurrentDayNetDepositAsync(query.BrandId)
				.ConfigureAwait(false);
		}

		if (query.ReportDate == yesterday && now < scheduleTime)
		{
			return await brandNetDepositRepository
				.GetBrandYesterDayNetDepositAsync(query.BrandId)
				.ConfigureAwait(false);
		}

		var netDeposit = await paymentRepository
			.GetBrandNetDepositAsync(query.BrandId, query.ReportDate, cancellationToken)
			.ConfigureAwait(false);

		return netDeposit == null
			? 0m
			: netDeposit.Value;
	}
}
