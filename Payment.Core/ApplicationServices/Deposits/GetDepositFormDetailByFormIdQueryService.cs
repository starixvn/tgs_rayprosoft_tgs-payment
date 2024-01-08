using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class GetDepositFormDetailByFormIdQueryService(IPaymentRepository paymentRepository) : IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo?>
{
	public ValueTask<DepositFormInfo?> GetAsync(GetDepositFormDetailByFormIdQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetDepositFormByIdAsync(query.FormId, cancellationToken);
}
