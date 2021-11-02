using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	internal class SubmitDepositFormThirdPartyOrderNoCommandService : ICommandService<SubmitDepositFormThirdPartyOrderNoCommand>
	{
		private readonly IPaymentClient _paymentClient;
		private readonly IPaymentRepository _paymentRepository;

		public SubmitDepositFormThirdPartyOrderNoCommandService(IPaymentRepository paymentRepository, IPaymentClient paymentClient)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		}

		public async ValueTask ExecuteAsync(SubmitDepositFormThirdPartyOrderNoCommand command, CancellationToken cancellationToken = default)
		{
			var formData = await _paymentRepository
				.GetDepositFormByIdAsync(command.FormId)
				.ConfigureAwait(false);

			if (formData == null)
				return;

			await _paymentRepository
				.SubmitDepositFormStatusAsync(
					command.FormId,
					formData.Status,
					null,
					null,
					command.ThirdPartyOrderNo,
					null)
				.ConfigureAwait(false);
		}
	}
}
