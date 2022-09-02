using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	internal class BrandPaymentDesignatedUsersExistQueryService : IQueryService<BrandPaymentDesignatedUsersExistQuery, BrandPaymentPlayerExistInfo>
	{
		private readonly IPaymentRepository _paymentRepository;

		public BrandPaymentDesignatedUsersExistQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public IAsyncEnumerable<BrandPaymentPlayerExistInfo> QueryAsync(BrandPaymentDesignatedUsersExistQuery query, CancellationToken cancellationToken = default) 
			=> _paymentRepository.CheckBrandPaymentDesignatedUsersExistAsync(query.Ids, cancellationToken);
	}
}
