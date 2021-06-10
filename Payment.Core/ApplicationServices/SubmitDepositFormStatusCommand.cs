using System;

namespace Tgs.Payment.ApplicationServices
{
	public record SubmitDepositFormStatusCommand(
		Guid FormId,
		PaymentFlowState Status,
		string? PaymentReturn);
}
