using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandPaymentFeeQueryService(IPaymentRepository paymentRepository) : IGetService<GetBrandPaymentFeeQuery, BrandPaymentFeeInfo>
{
	public ValueTask<BrandPaymentFeeInfo> GetAsync(GetBrandPaymentFeeQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetBrandPaymentFeeSettingAsync(query.Id, cancellationToken);
}
