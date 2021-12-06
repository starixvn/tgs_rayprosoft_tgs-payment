using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	internal class SignOffDepositFormCommandService : ICommandService<SignOffDepositFormCommand>
	{
		private readonly IPaymentClient _paymentClient;
		private readonly IPaymentRepository _paymentRepository;

		public SignOffDepositFormCommandService(
			IPaymentRepository paymentRepository,
			IPaymentClient paymentClient)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		}

		public async ValueTask ExecuteAsync(SignOffDepositFormCommand command, CancellationToken cancellationToken = default)
		{
			var formData = await _paymentRepository
				.GetDepositFormByIdAsync(command.FormId)
				.ConfigureAwait(false);

			if (formData == null)
				return;

			var formStatus = (PaymentFlowState)formData.Status;

			if (formStatus == PaymentFlowState.Approved || formStatus == PaymentFlowState.Reject)
				return;

			await _paymentRepository
				.SignOffDepositFormAsync(command.FormId, command.ReviewState, command.ReviewUserId, command.ReviewUserName, command.Remark)
				.ConfigureAwait(false);

			await _paymentClient
				.EmitDepositFormStatusChangedEventAsync(formData.Id, formData.BrandId)
				.ConfigureAwait(false);
		}
	}
}
