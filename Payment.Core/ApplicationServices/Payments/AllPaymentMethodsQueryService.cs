using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class AllPaymentMethodsQueryService : IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo>
{
	private readonly IPaymentRepository _paymentRepository;

	public AllPaymentMethodsQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public IAsyncEnumerable<PaymentMethodInfo> QueryAsync(
		AllPaymentMethodsQuery query,
		CancellationToken cancellationToken = default)
		=> _paymentRepository.GetAllPaymentMethodsAsync();
}
