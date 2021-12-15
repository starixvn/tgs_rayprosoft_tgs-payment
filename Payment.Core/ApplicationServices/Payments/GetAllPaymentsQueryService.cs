using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments
{
	internal class GetAllPaymentsQueryService : IQueryService<GetAllPaymentsQuery, PaymentInfo>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetAllPaymentsQueryService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public IAsyncEnumerable<PaymentInfo> QueryAsync(
            GetAllPaymentsQuery query,
            CancellationToken cancellationToken = default)
            => _paymentRepository.GetAllPaymentsAsync();
    }
}