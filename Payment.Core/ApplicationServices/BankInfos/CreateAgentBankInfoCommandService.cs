using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos
{
	internal class CreateAgentBankInfoCommandService : ICommandService<CreateAgentBankInfoCommand>
    {
        private readonly IPaymentRepository _paymentRepository;

        public CreateAgentBankInfoCommandService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public async ValueTask ExecuteAsync(CreateAgentBankInfoCommand command, CancellationToken cancellationToken = default)
        {
            var bankInfo = new AgentBankInfo(
                Guid.NewGuid(),
                command.AgentId,
                command.BrandId,
                command.PaymentChannelId,
                command.BankType,
                command.Detail);

            await _paymentRepository
                .CreateAgentBankInfoAsync(bankInfo, command.AgentName)
                .ConfigureAwait(false);

            command.TaskSource.SetResult(bankInfo.Id);
        }
    }
}