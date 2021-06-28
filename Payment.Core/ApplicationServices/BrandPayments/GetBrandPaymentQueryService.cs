using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	internal class GetBrandPaymentQueryService : IGetService<GetBrandPaymentQuery, BrandPaymentInfo>
	{
		private readonly IPaymentRepository _paymentRepository;

		public GetBrandPaymentQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public ValueTask<BrandPaymentInfo> GetAsync(GetBrandPaymentQuery query, CancellationToken cancellationToken = default)
		{
			return _paymentRepository.GetBrandPaymentByIdAsync(query.Id);
		}
	}
}
