using System;

namespace Tgs.Payment;

public record WithdrawFormInfo(
	Guid Id,
	string? UserId,
	UserType UserType,
	string? UserName,
	Guid BrandId,
	decimal Amount,
	decimal? ActualAmount,
	PaymentFlowState Status,
	string? Remark,
	string? ReviewedUserName,
	DateTimeOffset? ReviewedOn,
	string? OrderNo,
	Guid BankInfoId,
	DateTimeOffset CreatedOn,
	DateTimeOffset UpdatedOn,
	string? PaidUserId,
	string? PaidUserName,
	DateTimeOffset? PaidOn,
	decimal HandlingFee,
	Guid? BrandPaymentId = null,
	string? ChannelName = null,
	Guid? PaymentMethodId = null,
	string? BankInfo = null,
	string? ApplyInfo = null,
	string? PaymentReturn = null,
	string? ThirdPartyOrderNo = null,
	string? Note = null);
