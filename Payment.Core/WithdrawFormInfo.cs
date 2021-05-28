using System;

namespace Tgs.Payment
{
	public record WithdrawFormInfo(
		Guid Id,
		string PlayerCode,
		string UserName,
		Guid BrandId,
		decimal Amount,
		int Status,
		string? Remark,
		Guid BankInfoId,
		DateTimeOffset CreatedOn,
		DateTimeOffset UpdatedOn,
		Guid? BrandPaymentId = null,
		string? ChannelName = null,
		Guid? PaymentMethodId = null,
		string? BankInfo = null,
		string? ApplyInfo = null,
		string? PaymentReturn = null);
}
