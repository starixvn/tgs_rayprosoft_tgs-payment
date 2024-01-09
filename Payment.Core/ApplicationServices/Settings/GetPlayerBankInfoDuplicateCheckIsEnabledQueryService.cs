using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Settings;

internal class GetPlayerBankInfoDuplicateCheckIsEnabledQueryService(IPaymentRepository paymentRepository)
	: IGetService<GetPlayerBankInfoDuplicateCheckIsEnabledQuery, bool>
{
	public ValueTask<bool> GetAsync(GetPlayerBankInfoDuplicateCheckIsEnabledQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetBrandCheckPlayerBankInfoDuplicateIsEnabledAsync(query.BrandId, cancellationToken);
}
