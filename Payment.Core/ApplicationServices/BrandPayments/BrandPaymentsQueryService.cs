using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class BrandPaymentsQueryService(IPaymentRepository paymentRepository) : IQueryService<BrandPaymentsQuery, BrandPaymentInfo>
{
	public IAsyncEnumerable<BrandPaymentInfo> QueryAsync(BrandPaymentsQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.QueryBrandPaymentsAsync(
			query.BrandId,
			query.TransferType,
			query.Enabled,
			query.Keyword,
			cancellationToken);
}
