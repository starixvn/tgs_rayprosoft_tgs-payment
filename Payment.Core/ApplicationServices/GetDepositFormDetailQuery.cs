using System;

namespace Tgs.Payment.ApplicationServices
{
	public record GetDepositFormDetailQuery(
		Guid FormId,
		Guid BrandId);
}
