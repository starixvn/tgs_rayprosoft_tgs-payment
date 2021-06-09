using System;

namespace Tgs.Payment
{
	public record DepositFormInfo(
		Guid Id,
		string PlayerCode,
		string UserName,
		Guid BrandId,
		Guid BrandPaymentId,
		string ChannelName,
		Guid PaymentMethodId,
		decimal Amount,
		string? Remark,
		int Status,
		string? ApplyInfo,
		string? PaymentReturn,
		Guid? PromotionId,
		DateTimeOffset CreatedOn,
		DateTimeOffset UpdatedOn);
}
