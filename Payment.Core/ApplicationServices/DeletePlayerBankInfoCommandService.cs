using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices
{
	internal class DeletePlayerBankInfoCommandService : ICommandService<DeletePlayerBankInfoCommand>
	{
		private readonly IPaymentRepository _paymentRepository;

		public DeletePlayerBankInfoCommandService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public ValueTask ExecuteAsync(DeletePlayerBankInfoCommand command, CancellationToken cancellationToken = default)
			=> _paymentRepository.DeleteBankInfoAsync(command.PlayerCode, command.BankInfoId);
	}
}
