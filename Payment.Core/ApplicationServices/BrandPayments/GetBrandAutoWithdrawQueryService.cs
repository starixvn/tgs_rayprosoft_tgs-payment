using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandAutoWithdrawQueryService(IPaymentRepository paymentRepository)
	: IGetService<GetBrandAutoWithdrawQuery, AutoWithdrawInfo?>
{
	public ValueTask<AutoWithdrawInfo?> GetAsync(GetBrandAutoWithdrawQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetBrandAutoWithdrawSettingAsync(query.BrandId, cancellationToken);
}
