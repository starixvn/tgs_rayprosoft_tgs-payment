using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	internal class QueryBrandPaymentQueryService : IQueryService<QueryBrandPaymentQuery, BrandPaymentInfo>
	{
		private readonly IPaymentRepository _paymentRepository;

		public QueryBrandPaymentQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public IAsyncEnumerable<BrandPaymentInfo> QueryAsync(QueryBrandPaymentQuery query, CancellationToken cancellationToken = default)
		{
			return _paymentRepository.QueryBrandPaymentsAsync(query.BrandId, query.TransferType, query.Enabled, query.Keyword, cancellationToken);
		}
	}
}
