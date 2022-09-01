using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	internal class BrandPaymentsQueryService : IQueryService<BrandPaymentsQuery, BrandPaymentInfo>
	{
		private readonly IPaymentRepository _paymentRepository;

		public BrandPaymentsQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public IAsyncEnumerable<BrandPaymentInfo> QueryAsync(BrandPaymentsQuery query, CancellationToken cancellationToken = default)
			=> _paymentRepository.QueryBrandPaymentsAsync(
				query.BrandId,
				query.TransferType,
				query.Enabled,
				query.Keyword,
				cancellationToken);
	}
}
