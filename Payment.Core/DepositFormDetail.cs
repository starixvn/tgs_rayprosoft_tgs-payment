using System;

namespace Tgs.Payment
{
	public record DepositFormDetail(
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
		DateTimeOffset CreatedOn,
		DateTimeOffset UpdatedOn);
}
