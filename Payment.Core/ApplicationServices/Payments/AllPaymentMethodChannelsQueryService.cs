using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class AllPaymentMethodChannelsQueryService : IQueryService<AllPaymentMethodChannelsQuery, PaymentMethodChannelInfo>
{
	private readonly IPaymentRepository _paymentRepository;

	public AllPaymentMethodChannelsQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public IAsyncEnumerable<PaymentMethodChannelInfo> QueryAsync(
		AllPaymentMethodChannelsQuery query,
		CancellationToken cancellationToken = default)
		=> _paymentRepository.GetAllPaymentMethodChannelsAsync();
}
