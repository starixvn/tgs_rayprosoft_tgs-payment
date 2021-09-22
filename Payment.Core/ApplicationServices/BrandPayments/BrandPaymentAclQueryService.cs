using System;
using System.Collections.Generic;
using System.Threading;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	internal class BrandPaymentAclQueryService : IQueryService<BrandPaymentAclQuery, BrandPaymentAcl>
    {
        private readonly IPaymentRepository _paymentRepository;

        public BrandPaymentAclQueryService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public IAsyncEnumerable<BrandPaymentAcl> QueryAsync(
            BrandPaymentAclQuery query,
            CancellationToken cancellationToken = default)
            => _paymentRepository.GetBrandPaymentAclsAsync(query.BrandPaymentIds, query.PlayerCode);
    }
}