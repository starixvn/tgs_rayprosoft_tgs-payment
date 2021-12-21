using System;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	public record RejectDepositFormCommand(Guid FormId, Guid BrandId);
}
