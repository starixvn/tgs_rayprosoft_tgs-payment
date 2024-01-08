using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class AllPaymentMethodChannelsQueryService(IPaymentRepository paymentRepository) : IQueryService<AllPaymentMethodChannelsQuery, PaymentMethodChannelInfo>
{
	public IAsyncEnumerable<PaymentMethodChannelInfo> QueryAsync(
		AllPaymentMethodChannelsQuery query,
		CancellationToken cancellationToken = default)
		=> paymentRepository.GetAllPaymentMethodChannelsAsync(cancellationToken);
}
