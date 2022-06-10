using System;
using System.Threading.Tasks;

namespace Tgs.Payment.ApplicationServices.BankInfos;

public record CreatePlayerBankInfoCommand(
	Guid BrandId,
	string PlayerCode,
	Guid PaymentChannelId,
	string BankType,
	string Detail,
	bool IsDefault,
	string ModifyUser)
{
	public TaskCompletionSource<Guid> TaskSource { get; } = new TaskCompletionSource<Guid>();
}
