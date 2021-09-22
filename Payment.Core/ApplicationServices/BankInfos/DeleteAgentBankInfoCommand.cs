using System;

namespace Tgs.Payment.ApplicationServices.BankInfos
{
	public record DeleteAgentBankInfoCommand(
        Guid AgentId,
        Guid BankInfoId);
}