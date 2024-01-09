using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class GetDepositFormDetailQueryService(IPaymentRepository paymentRepository) : IGetService<GetDepositFormDetailQuery, DepositFormInfo?>
{
	public async ValueTask<DepositFormInfo?> GetAsync(GetDepositFormDetailQuery query, CancellationToken cancellationToken = default)
	{
		var formData = await paymentRepository
			.GetDepositFormByIdAsync(query.FormId, cancellationToken)
			.ConfigureAwait(false);

		return formData?.BrandId != query.BrandId ? null : formData;
	}
}
