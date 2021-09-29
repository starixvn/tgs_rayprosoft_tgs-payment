using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.Services
{
	internal class SaveWithdrawThirdPartyOrderNoCommandService : ICommandService<SaveWithdrawThirdPartyOrderNoCommand>
	{
		private readonly IPaymentRepository _paymentRepository;

		public SaveWithdrawThirdPartyOrderNoCommandService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public async ValueTask ExecuteAsync(SaveWithdrawThirdPartyOrderNoCommand command, CancellationToken cancellationToken = default)
			=> await _paymentRepository.SubmitWithdrawFormThirdPartyOrderNoAsync(command.FormId, command.ThirdPartyOrderNo);
	}
}