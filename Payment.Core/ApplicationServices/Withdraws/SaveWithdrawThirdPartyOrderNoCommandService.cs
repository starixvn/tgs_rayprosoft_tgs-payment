using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws
{
	internal class SaveWithdrawThirdPartyOrderNoCommandService : ICommandService<SaveWithdrawThirdPartyOrderNoCommand>
	{
		private readonly IPaymentRepository _paymentRepository;

		public SaveWithdrawThirdPartyOrderNoCommandService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public ValueTask ExecuteAsync(SaveWithdrawThirdPartyOrderNoCommand command, CancellationToken cancellationToken = default)
			=> _paymentRepository.SubmitWithdrawFormThirdPartyOrderNoAsync(command.FormId, command.ThirdPartyOrderNo);
	}
}
