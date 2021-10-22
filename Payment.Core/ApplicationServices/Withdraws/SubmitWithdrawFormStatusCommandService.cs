using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws
{
	internal class SubmitWithdrawFormStatusCommandService : ICommandService<SubmitWithdrawFormStatusCommand>
	{
		private readonly IPaymentClient _paymentClient;
		private readonly IPaymentRepository _paymentRepository;

		public SubmitWithdrawFormStatusCommandService(
			IPaymentRepository paymentRepository,
			IPaymentClient paymentClient)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		}

		public async ValueTask ExecuteAsync(SubmitWithdrawFormStatusCommand command, CancellationToken cancellationToken = default)
		{
			var formData = await _paymentRepository
				.GetWithdrawFormByIdAsync(command.FormId)
				.ConfigureAwait(false);

			if (formData == null)
				return;

			var originStatus = formData.Status;

			await _paymentRepository
				.SubmitWithdrawFormStatusAsync(
					command.FormId,
					command.Status,
					command.PaymentReturn,
					command.ActualAmount,
					command.ThirdPartyOrderNo)
				.ConfigureAwait(false);

			if (originStatus != command.Status)
				await _paymentClient
					.EmitWithdrawFormStatusChangedEventAsync(command.FormId, formData.BrandId)
					.ConfigureAwait(false);
		}
	}
}
