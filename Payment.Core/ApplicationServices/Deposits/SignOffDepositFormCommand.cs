using System;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	public record SignOffDepositFormCommand(
		Guid FormId,
		ReviewState ReviewState,
		string? Remark);
}
