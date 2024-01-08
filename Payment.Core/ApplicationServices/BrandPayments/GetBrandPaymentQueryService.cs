using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandPaymentQueryService(IPaymentRepository paymentRepository) : IGetService<GetBrandPaymentQuery, BrandPaymentInfo>
{
	public ValueTask<BrandPaymentInfo> GetAsync(GetBrandPaymentQuery query, CancellationToken cancellationToken = default) 
		=> paymentRepository.GetBrandPaymentByIdAsync(query.Id, cancellationToken);
}
