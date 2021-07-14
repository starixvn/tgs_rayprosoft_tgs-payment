using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos
{
	internal class DeleteAgentBankInfoCommandService : ICommandService<DeleteAgentBankInfoCommand>
	{
		private readonly IPaymentRepository _paymentRepository;

		public DeleteAgentBankInfoCommandService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public ValueTask ExecuteAsync(DeleteAgentBankInfoCommand command, CancellationToken cancellationToken = default)
			=> _paymentRepository.DeleteAgentBankInfoAsync(command.AgentId, command.BankInfoId);
	}
}
