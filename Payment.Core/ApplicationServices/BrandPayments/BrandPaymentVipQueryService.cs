using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class BrandPaymentVipQueryService(IPaymentRepository paymentRepository) : IQueryService<BrandPaymentVipQuery, BrandPaymentVip>
{
	public IAsyncEnumerable<BrandPaymentVip> QueryAsync(
		BrandPaymentVipQuery query,
		CancellationToken cancellationToken = default)
		=> paymentRepository.GetBrandPaymentVipListAsync(query.BrandPaymentIds, cancellationToken);
}
