using System;

namespace Tgs.Payment.ApplicationServices
{
	public record SubmitWithdrawFormStatusCommand(
		Guid FormId,
		PaymentFlowState Status,
		string? PaymentReturn,
		decimal? ActualAmount);
}
