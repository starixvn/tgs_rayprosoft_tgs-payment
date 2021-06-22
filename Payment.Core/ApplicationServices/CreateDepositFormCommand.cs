using System;

namespace Tgs.Payment.ApplicationServices
{
	public record CreateDepositFormCommand(Guid FormId, DepositFormCreateInfo CreateInfo);
}
