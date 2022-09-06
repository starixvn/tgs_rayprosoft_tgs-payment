using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	internal class CalculateDepositFeeCommandService : IGetService<CalculateDepositFeeCommand, decimal>
	{
		private readonly IPaymentRepository _paymentRepository;

		public CalculateDepositFeeCommandService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public async ValueTask<decimal> GetAsync(CalculateDepositFeeCommand query, CancellationToken cancellationToken = default)
		{
			var form = await _paymentRepository
				.GetDepositFormByIdAsync(query.FormId)
				.ConfigureAwait(false);

			var brandPaymentFeeInfo = await _paymentRepository
				.GetBrandPaymentFeeSettingsAsync(form!.BrandPaymentId)
				.ConfigureAwait(false);

			if (brandPaymentFeeInfo.ReducePlayerAmount is false)
				return 0m;

			if (query.ActualAmount != null)
				return query.ActualAmount.Value * brandPaymentFeeInfo.HandlingFeeRateTenPercentile * 0.1m;

			return query.Amount * brandPaymentFeeInfo.HandlingFeeRateTenPercentile * 0.1m;
		}
	}
}
