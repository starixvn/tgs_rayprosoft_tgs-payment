using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class BrandPaymentDesignatedUserQueryService(IPaymentRepository paymentRepository)
	: IQueryService<BrandPaymentDesignatedUserQuery, BrandPaymentPlayerInfo>
{
	public IAsyncEnumerable<BrandPaymentPlayerInfo> QueryAsync(BrandPaymentDesignatedUserQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetBrandPaymentDesignatedUsersAsync(query.Id, cancellationToken);
}
