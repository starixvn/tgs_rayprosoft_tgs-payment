using System;

namespace Tgs.Payment;
public record DepositFormSnapshot(
	Guid Id,
	string PlayerCode,
	Guid BrandId,
	Guid BrandPaymentId,
	decimal Amount,
	decimal? ActualAmount,
	string? Remark,
	PaymentFlowState Status,
	string? ApplyInfo,
	string? PaymentReturn,
	string? ThirdPartyOrderNo,
	string? OrderNo,
	string? ReviewedUserId,
	string? ReviewedUserName,
	DateTime? ReviewedOn,
	Guid? PromotionId,
	int? PromotionPlayerId,
	DateTime CreatedOn,
	decimal HandlingFee);
