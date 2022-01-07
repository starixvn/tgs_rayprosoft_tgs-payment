using System;

namespace Tgs.Payment.ApplicationServices.Deposits;

public record SubmitDepositFormStatusCommand(
	Guid FormId,
	PaymentFlowState? Status,
	string? PaymentReturn,
	decimal? ActualAmount,
	string? ThirdPartyOrderNo,
	int? PromotionPlayerId);
