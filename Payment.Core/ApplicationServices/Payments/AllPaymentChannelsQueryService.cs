using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class AllPaymentChannelsQueryService(IPaymentRepository paymentRepository) : IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo>
{
	public IAsyncEnumerable<PaymentChannelInfo> QueryAsync(
		AllPaymentChannelsQuery query,
		CancellationToken cancellationToken = default)
		=> paymentRepository.GetAllPaymentChannelsAsync(cancellationToken);
}
