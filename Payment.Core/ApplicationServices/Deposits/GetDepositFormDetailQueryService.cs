using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class GetDepositFormDetailQueryService : IGetService<GetDepositFormDetailQuery, DepositFormInfo?>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetDepositFormDetailQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public async ValueTask<DepositFormInfo?> GetAsync(GetDepositFormDetailQuery query, CancellationToken cancellationToken = default)
	{
		var formData = await _paymentRepository
			.GetDepositFormByIdAsync(query.FormId, cancellationToken)
			.ConfigureAwait(false);

		return formData?.BrandId != query.BrandId ? null : formData;
	}
}
