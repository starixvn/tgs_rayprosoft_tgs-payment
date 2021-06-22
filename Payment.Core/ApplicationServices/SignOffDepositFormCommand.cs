using System;

namespace Tgs.Payment.ApplicationServices
{
	public record SignOffDepositFormCommand(
		Guid FormId,
		ReviewState ReviewState,
		string? Remark);
}
