using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandReportDateNetDepositQueryService : IGetService<GetBrandReportDateNetDepositQuery, decimal>
{
	private readonly IBrandNetDepositRepository _brandNetDepositRepository;
	private readonly IPaymentRepository _paymentRepository;

	public GetBrandReportDateNetDepositQueryService(
		IPaymentRepository paymentRepository,
		IBrandNetDepositRepository brandNetDepositRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_brandNetDepositRepository = brandNetDepositRepository ?? throw new ArgumentNullException(nameof(brandNetDepositRepository));
	}

	public async ValueTask<decimal> GetAsync(GetBrandReportDateNetDepositQuery query, CancellationToken cancellationToken = default)
	{
		var netDeposit = await _paymentRepository
			.GetBrandNetDepositAsync(query.BrandId, query.ReportDate, cancellationToken)
			.ConfigureAwait(false);

		if (netDeposit == null)
		{
			return await _brandNetDepositRepository
				.GetBrandCurrentDayNetDepositAsync(query.BrandId)
				.ConfigureAwait(false);
		}

		return netDeposit.Value;
	}
}
