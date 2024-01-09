using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class BrandPaymentQueryService(IPaymentRepository paymentRepository) : IQueryService<BrandPaymentQuery, BrandPaymentInfo>
{
	public IAsyncEnumerable<BrandPaymentInfo> QueryAsync(
		BrandPaymentQuery query,
		CancellationToken cancellationToken = default)
		=> paymentRepository.GetBrandPaymentsAsync(query.BrandId, cancellationToken);
}
