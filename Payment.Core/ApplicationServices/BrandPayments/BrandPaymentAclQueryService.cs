using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class BrandPaymentAclQueryService(IPaymentRepository paymentRepository) : IQueryService<BrandPaymentAclQuery, BrandPaymentAcl>
{
	public IAsyncEnumerable<BrandPaymentAcl> QueryAsync(
		BrandPaymentAclQuery query,
		CancellationToken cancellationToken = default)
		=> paymentRepository.GetBrandPaymentAclsAsync(query.BrandPaymentIds, query.PlayerCode, cancellationToken);
}
