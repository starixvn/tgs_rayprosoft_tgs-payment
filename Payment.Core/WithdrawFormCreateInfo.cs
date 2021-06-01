using System;

namespace Tgs.Payment
{
	public record WithdrawFormCreateInfo(
		string UserId,
		UserType UserType,
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
