using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class SuccessRateQueryService : IGetService<SuccessRateQuery, decimal>
{
	private readonly IPaymentRepository _paymentRepository;

	public SuccessRateQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public async ValueTask<decimal> GetAsync(SuccessRateQuery query, CancellationToken cancellationToken = default)
	{
		var result = await _paymentRepository
			.CountCurrentDayDepositFormStatusAsync(query.BrandId, cancellationToken)
			.ConfigureAwait(false);

		return result.Total is 0
			? 0m
			: Math.Round((decimal)result.Approved / result.Total * 100, MidpointRounding.AwayFromZero);
	}
}
