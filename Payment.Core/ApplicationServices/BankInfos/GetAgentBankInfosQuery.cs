using System;

namespace Tgs.Payment.ApplicationServices.BankInfos
{
    public record GetAgentBankInfosQuery(
        Guid AgentId);
}
