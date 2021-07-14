using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws
{
	internal class GetWithdrawFormDetailQueryService : IGetService<GetWithdrawFormDetailQuery, WithdrawFormInfo?>
	{
		private readonly IPaymentRepository _paymentRepository;

		public GetWithdrawFormDetailQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public async ValueTask<WithdrawFormInfo?> GetAsync(
			GetWithdrawFormDetailQuery query,
			CancellationToken cancellationToken = default)
		{
			var formData = await _paymentRepository
				.GetWithdrawFormByIdAsync(query.FormId)
				.ConfigureAwait(false);

			if (formData?.BrandId != query.BrandId)
				return null;

			return formData;
		}
	}
}
