using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Players;

internal class GetPaymentOverviewQueryService : IGetService<GetPaymentOverviewQuery, PaymentOverview?>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetPaymentOverviewQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new System.ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask<PaymentOverview?> GetAsync(GetPaymentOverviewQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.GetPlayerPaymentOverviewAsync(query.PlayerCode, cancellationToken);
}
