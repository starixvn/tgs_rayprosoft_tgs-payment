using System;

namespace Tgs.Payment.ApplicationServices.Withdraws
{
	public record SubmitWithdrawFormStatusCommand(
		Guid FormId,
		PaymentFlowState? Status,
		string? PaymentReturn,
		decimal? ActualAmount,
		string? ThirdPartyOrderNo,
		string? PaidUserId,
		string? PaidUserName);
}
