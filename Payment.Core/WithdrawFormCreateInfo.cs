using System;

namespace Tgs.Payment
{
	public record WithdrawFormCreateInfo(
		string PlayerCode,
		Guid BrandId,
		decimal Amount,
		int Status,
		string? Remark,
		Guid BankInfoId,
		Guid? BrandPaymentId = null,
		string? BankInfo = null,
		string? ApplyInfo = null,
		string? PaymentReturn = null);
}
