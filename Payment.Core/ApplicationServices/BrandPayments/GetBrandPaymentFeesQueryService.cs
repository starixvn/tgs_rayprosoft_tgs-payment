using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandPaymentFeesQueryService(IPaymentRepository paymentRepository) : IQueryService<GetBrandPaymentFeesQuery, BrandPaymentFeeInfo>
{
	public IAsyncEnumerable<BrandPaymentFeeInfo> QueryAsync(GetBrandPaymentFeesQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetBrandPaymentFeeSettingsAsync(query.BrandPaymentIds, cancellationToken);
}
