using System;

namespace Tgs.Payment.ApplicationServices
{
	public record GetWithdrawFormDetailQuery(
		Guid FormId,
		Guid BrandId);
}
