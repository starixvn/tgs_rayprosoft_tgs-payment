using System;

namespace Tgs.Payment.ApplicationServices
{
	public record SignOffWithdrawFormCommand(
		Guid FormId,
		Guid? BrandPaymentId,
		ReviewState Status,
		string? Remark);
}
