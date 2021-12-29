using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	internal class SubmitDepositFormStatusCommandService : ICommandService<SubmitDepositFormStatusCommand>
	{
		private readonly IPaymentClient _paymentClient;
		private readonly IPaymentRepository _paymentRepository;

		public SubmitDepositFormStatusCommandService(
			IPaymentRepository paymentRepository,
			IPaymentClient paymentClient)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		}

		public async ValueTask ExecuteAsync(SubmitDepositFormStatusCommand command, CancellationToken cancellationToken = default)
		{
			var formData = await _paymentRepository
				.GetDepositFormByIdAsync(command.FormId)
				.ConfigureAwait(false);

			if (formData == null)
				return;

			var originStatus = formData.Status;

			await _paymentRepository
				.SubmitDepositFormStatusAsync(
					command.FormId,
					command.Status,
					command.PaymentReturn,
					command.ActualAmount,
					command.ThirdPartyOrderNo,
					command.PromotionPlayerId)
				.ConfigureAwait(false);

			if (originStatus != command.Status)
				await _paymentClient
					.EmitDepositFormStatusChangedEventAsync(command.FormId, formData.BrandId, originStatus.ToString())
					.ConfigureAwait(false);
		}
	}
}
