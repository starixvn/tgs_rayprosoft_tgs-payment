using System;
using System.Threading.Tasks;

namespace Tgs.Payment.ApplicationServices.BankInfos;

public record CreateAgentBankInfoCommand(
	Guid BrandId,
	Guid AgentId,
	string AgentName,
	Guid PaymentChannelId,
	string BankType,
	string Detail)
{
	public TaskCompletionSource<Guid> TaskSource { get; } = new TaskCompletionSource<Guid>();
}
