using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class GetAllPaymentsQueryService(IPaymentRepository paymentRepository) : IQueryService<GetAllPaymentsQuery, PaymentInfo>
{
	public IAsyncEnumerable<PaymentInfo> QueryAsync(
		GetAllPaymentsQuery query,
		CancellationToken cancellationToken = default)
		=> paymentRepository.GetAllPaymentsAsync(cancellationToken);
}
