using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class AllPaymentMethodsQueryService(IPaymentRepository paymentRepository) : IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo>
{
	public IAsyncEnumerable<PaymentMethodInfo> QueryAsync(
		AllPaymentMethodsQuery query,
		CancellationToken cancellationToken = default)
		=> paymentRepository.GetAllPaymentMethodsAsync(cancellationToken);
}
