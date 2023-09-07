using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class ReportDateSuccessRateQueryService : IGetService<ReportDateSuccessRateQuery, decimal>
{
	private readonly IPaymentRepository _paymentRepository;

	public ReportDateSuccessRateQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public async ValueTask<decimal> GetAsync(ReportDateSuccessRateQuery query, CancellationToken cancellationToken = default)
	{
		var reportData = await _paymentRepository
			.GetReportDateDepositFormSummaryAsync(query.BrandId, query.ReportDate, cancellationToken)
			.ConfigureAwait(false);

		return reportData.Total is 0
			? 0m
			: Math.Round((decimal)reportData.Approved / reportData.Total * 100, MidpointRounding.AwayFromZero);
	}
}
