using System;
using System.Collections.Generic;
using System.Threading;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices
{
	internal class BrandPaymentVipQueryService : IQueryService<BrandPaymentVipQuery, BrandPaymentVip>
	{
		private readonly IPaymentRepository _paymentRepository;

		public BrandPaymentVipQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public IAsyncEnumerable<BrandPaymentVip> QueryAsync(
			BrandPaymentVipQuery query,
			CancellationToken cancellationToken = default)
			=> _paymentRepository.GetBrandPaymentVipListAsync(query.BrandPaymentIds);
	}
}
