using System;

namespace Tgs.Payment;

public record WithdrawFormCreateInfo(
	string? UserId,
	string? UserName,
	UserType UserType,
	Guid BrandId,
	decimal Amount,
	PaymentFlowState Status,
	string? Remark,
	Guid BankInfoId,
	string BankInfo,
	Guid? BrandPaymentId = null,
	string? ApplyInfo = null,
	string? PaymentReturn = null,
	string? OrderNo = null);
