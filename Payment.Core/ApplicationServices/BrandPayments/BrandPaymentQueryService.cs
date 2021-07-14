using System;
using System.Collections.Generic;
using System.Threading;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	internal class BrandPaymentQueryService : IQueryService<BrandPaymentQuery, BrandPaymentInfo>
	{
		private readonly IPaymentRepository _paymentRepository;

		public BrandPaymentQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public IAsyncEnumerable<BrandPaymentInfo> QueryAsync(
			BrandPaymentQuery query,
			CancellationToken cancellationToken = default)
			=> _paymentRepository.GetBrandPaymentsAsync(query.BrandId);
	}
}
