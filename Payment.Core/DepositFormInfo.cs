using System;

namespace Tgs.Payment;

public record DepositFormInfo(
	Guid Id,
	string PlayerCode,
	string UserName,
	Guid BrandId,
	Guid BrandPaymentId,
	string ChannelName,
	Guid PaymentMethodId,
	decimal Amount,
	decimal? ActualAmount,
	string? Remark,
	PaymentFlowState Status,
	string? ApplyInfo,
	string? PaymentReturn,
	string? ThirdPartyOrderNo,
	string? OrderNo,
	string? ReviewedUserName,
	DateTimeOffset? ReviewedOn,
	Guid? PromotionId,
	int? PromotionPlayerId,
	DateTimeOffset CreatedOn,
	DateTimeOffset UpdatedOn,
	decimal HandlingFee);
