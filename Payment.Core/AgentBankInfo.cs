using System;

namespace Tgs.Payment
{
	public record AgentBankInfo(
        Guid Id,
        Guid AgentId,
        Guid BrandId,
        Guid PaymentChannelId,
        string BankType,
        string Detail);
}