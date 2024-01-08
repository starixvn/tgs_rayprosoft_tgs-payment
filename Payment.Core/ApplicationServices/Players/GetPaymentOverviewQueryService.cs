using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Players;

internal class GetPaymentOverviewQueryService(IPaymentRepository paymentRepository)
	: IGetService<GetPaymentOverviewQuery, PaymentOverview?>
{

	public ValueTask<PaymentOverview?> GetAsync(GetPaymentOverviewQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetPlayerPaymentOverviewAsync(query.PlayerCode, cancellationToken);
}
