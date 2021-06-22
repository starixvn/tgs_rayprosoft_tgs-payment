using System;

namespace Tgs.Payment
{
	public record WithdrawFormCreateInfo(
		string? UserId,
		string? UserName,
		UserType UserType,
		Guid BrandId,
		decimal Amount,
		PaymentFlowState Status,
		string? Remark,
		Guid BankInfoId,
		Guid? BrandPaymentId = null,
		string? BankInfo = null,
		string? ApplyInfo = null,
		string? PaymentReturn = null);
}
