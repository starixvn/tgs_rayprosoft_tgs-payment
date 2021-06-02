using System;

namespace Tgs.Payment
{
	public record DepositFormInfo(
		string PlayerCode,
		Guid BrandId,
		Guid BrandPaymentId,
		decimal Amount,
		Guid? PromotionId,
		string? ApplyInfo,
		string? PaymentReturn = null);
}
