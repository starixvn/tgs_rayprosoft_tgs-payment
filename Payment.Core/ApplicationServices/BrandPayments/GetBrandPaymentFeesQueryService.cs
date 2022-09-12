using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	internal class GetBrandPaymentFeesQueryService : IQueryService<GetBrandPaymentFeesQuery, BrandPaymentFeeInfo>
	{
		private readonly IPaymentRepository _paymentRepository;

		public GetBrandPaymentFeesQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public IAsyncEnumerable<BrandPaymentFeeInfo> QueryAsync(GetBrandPaymentFeesQuery query, CancellationToken cancellationToken = default)
			=> _paymentRepository.GetBrandPaymentsFeeSettingsAsync(query.BrandPaymentIds, cancellationToken);
	}
}
