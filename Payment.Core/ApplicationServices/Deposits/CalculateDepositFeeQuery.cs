using System;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	public record CalculateDepositFeeQuery(Guid FormId, decimal Amount, decimal? ActualAmount);
}
