using System;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	public record GetDepositFormDetailQuery(
        Guid FormId,
        Guid BrandId);
}