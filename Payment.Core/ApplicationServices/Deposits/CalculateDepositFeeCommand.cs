using System;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	public record CalculateDepositFeeCommand(Guid FormId, decimal Amount, decimal? ActualAmount);
}
