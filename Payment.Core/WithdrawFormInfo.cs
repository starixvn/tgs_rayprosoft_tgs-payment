using System;

namespace Tgs.Payment
{
	public record WithdrawFormInfo(
		Guid Id,
		string? UserId,
		UserType UserType,
		string? UserName,
		Guid BrandId,
		decimal Amount,
		PaymentFlowState Status,
		string? Remark,
		string? ReviewedUserName,
		Guid BankInfoId,
		DateTimeOffset CreatedOn,
		DateTimeOffset UpdatedOn,
		Guid? BrandPaymentId = null,
		string? ChannelName = null,
		Guid? PaymentMethodId = null,
		string? BankInfo = null,
		string? ApplyInfo = null,
		string? PaymentReturn = null,
		string? ThirdPartyOrderNo = null);
}