using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandReportDateNetDepositQueryService : IGetService<GetBrandReportDateNetDepositQuery, decimal>
{
	private readonly IBrandNetDepositRepository _brandNetDepositRepository;
	private readonly ISystemClock _systemClock;
	private readonly IPaymentRepository _paymentRepository;

	public GetBrandReportDateNetDepositQueryService(
		ISystemClock systemClock,
		IPaymentRepository paymentRepository,
		IBrandNetDepositRepository brandNetDepositRepository)
	{
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_brandNetDepositRepository = brandNetDepositRepository ?? throw new ArgumentNullException(nameof(brandNetDepositRepository));
	}

	public async ValueTask<decimal> GetAsync(GetBrandReportDateNetDepositQuery query, CancellationToken cancellationToken = default)
	{
		var now = _systemClock.UtcNow.ToOffset(TimeSpan.FromHours(8D));
		var currentDate = DateOnly.FromDateTime(now.DateTime);
		var yesterday = currentDate.AddDays(-1);

		var scheduleTime = new DateTimeOffset(now.Year, now.Month, now.Day, 8, 0, 0, TimeSpan.FromHours(8));

		if (query.ReportDate == currentDate)
		{
			return await _brandNetDepositRepository
				.GetBrandCurrentDayNetDepositAsync(query.BrandId)
				.ConfigureAwait(false);
		}

		if(query.ReportDate == yesterday && now < scheduleTime)
		{
			return await _brandNetDepositRepository
				.GetBrandYesterDayNetDepositAsync(query.BrandId)
				.ConfigureAwait(false);
		}

		var netDeposit = await _paymentRepository
			.GetBrandNetDepositAsync(query.BrandId, query.ReportDate, cancellationToken)
			.ConfigureAwait(false);

		return netDeposit == null
			? 0m
			: netDeposit.Value;
	}
}
