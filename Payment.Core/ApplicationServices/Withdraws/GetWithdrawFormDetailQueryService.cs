using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class GetWithdrawFormDetailQueryService(IPaymentRepository paymentRepository) : IGetService<GetWithdrawFormDetailQuery, WithdrawFormInfo?>
{
	public async ValueTask<WithdrawFormInfo?> GetAsync(
		GetWithdrawFormDetailQuery query,
		CancellationToken cancellationToken = default)
	{
		var formData = await paymentRepository
			.GetWithdrawFormByIdAsync(query.FormId, cancellationToken)
			.ConfigureAwait(false);

		return formData?.BrandId != query.BrandId ? null : formData;
	}
}
