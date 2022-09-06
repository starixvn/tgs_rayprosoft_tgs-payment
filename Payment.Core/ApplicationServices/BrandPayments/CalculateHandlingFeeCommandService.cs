using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	internal class CalculateHandlingFeeCommandService : IGetService<CalculateHandlingFeeCommand, decimal>
	{
		private readonly IPaymentRepository _paymentRepository;

		public CalculateHandlingFeeCommandService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public async ValueTask<decimal> GetAsync(CalculateHandlingFeeCommand command, CancellationToken cancellationToken = default)
		{
			var brandPaymentFeeInfo = await _paymentRepository
				.GetBrandPaymentFeeSettingsAsync(command.BrandPaymentId)
				.ConfigureAwait(false);

			if (brandPaymentFeeInfo.ReducePlayerAmount is false)
				return 0m;

			return command.Amount * brandPaymentFeeInfo.HandlingFeeRateTenPercentile * 0.001m;
		}
	}
}
