using System;
using System.Threading.Tasks;

namespace Tgs.Payment.ApplicationServices.Withdraws;

public record CreateWithdrawFormCommand(
	string? UserId,
	string? UserName,
	UserType UserType,
	Guid BrandId,
	Guid BankInfoId,
	decimal Amount,
	string? Note)
{
	public TaskCompletionSource<Guid> ReturnFormIdSource { get; } = new TaskCompletionSource<Guid>();
}
