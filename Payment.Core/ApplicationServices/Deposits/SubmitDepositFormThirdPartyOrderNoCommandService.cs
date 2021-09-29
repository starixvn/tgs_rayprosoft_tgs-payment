using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	internal class SubmitDepositFormThirdPartyOrderNoCommandService : ICommandService<SubmitDepositFormThirdPartyOrderNoCommand>
	{
		private readonly IPaymentRepository _paymentRepository;

		public SubmitDepositFormThirdPartyOrderNoCommandService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public async ValueTask ExecuteAsync(SubmitDepositFormThirdPartyOrderNoCommand command, CancellationToken cancellationToken = default)
		{
			await _paymentRepository
				.SubmitDepositFormThirdPartyOrderNoAsync(
					command.FormId,
					command.ThirdPartyOrderNo)
				.ConfigureAwait(false);
		}
	}
}