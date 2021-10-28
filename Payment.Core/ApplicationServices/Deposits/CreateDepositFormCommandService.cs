using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	internal class CreateDepositFormCommandService : ICommandService<CreateDepositFormCommand>
	{
		private readonly IPaymentClient _paymentClient;
		private readonly IPaymentRepository _paymentRepository;

		public CreateDepositFormCommandService(
			IPaymentRepository paymentRepository,
			IPaymentClient paymentClient)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		}

		public async ValueTask ExecuteAsync(CreateDepositFormCommand command, CancellationToken cancellationToken = default)
		{
			await _paymentRepository
				.CreateDepositFormAsync(command.FormId, command.CreateInfo)
				.ConfigureAwait(false);

			await _paymentRepository
				.SubmitDepositFormStatusAsync(command.FormId, PaymentFlowState.WaitPayment, null, null, null)
				.ConfigureAwait(false);

			await _paymentClient
				.EmitDepositFormCreatedEventAsync(command.FormId, command.CreateInfo.BrandId)
				.ConfigureAwait(false);
		}
	}
}
