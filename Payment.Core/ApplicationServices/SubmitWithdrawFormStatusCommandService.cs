using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices
{
	class SubmitWithdrawFormStatusCommandService : ICommandService<SubmitWithdrawFormStatusCommand>
	{
		private readonly IPaymentRepository _paymentRepository;
		private readonly IPaymentClient _paymentClient;

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

			await _paymentRepository
				.SubmitWithdrawFormStatusAsync(
					command.FormId,
					command.Status,
					command.PaymentReturn,
					command.ActualAmount)
				.ConfigureAwait(false);

			await _paymentClient
				.EmitWithdrawFormStatusChangedEventAsync(command.FormId, formData.BrandId)
				.ConfigureAwait(false);
		}
	}
}
