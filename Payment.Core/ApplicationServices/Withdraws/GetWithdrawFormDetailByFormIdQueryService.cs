using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class GetWithdrawFormDetailByFormIdQueryService(IPaymentRepository paymentRepository)
	: IGetService<GetWithdrawFormDetailByFormIdQuery, WithdrawFormInfo?>
{
	public ValueTask<WithdrawFormInfo?> GetAsync(
		GetWithdrawFormDetailByFormIdQuery query,
		CancellationToken cancellationToken = default)
		=> paymentRepository.GetWithdrawFormByIdAsync(query.FormId, cancellationToken);
}
