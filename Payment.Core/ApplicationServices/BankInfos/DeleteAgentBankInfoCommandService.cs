using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class DeleteAgentBankInfoCommandService(IPaymentRepository paymentRepository) : ICommandService<DeleteAgentBankInfoCommand>
{
	public ValueTask ExecuteAsync(DeleteAgentBankInfoCommand command, CancellationToken cancellationToken = default)
		=> paymentRepository.DeleteAgentBankInfoAsync(command.AgentId, command.BankInfoId, cancellationToken);
}
