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
	decimal HandlingFee)
{
	internal static readonly DepositFormSnapshot _Default = new(
		Guid.Empty,
		string.Empty,
		Guid.Empty,
		Guid.Empty,
		0M,
		null,
		null,
		PaymentFlowState.Init,
		null,
		null,
		null,
		null,
		null,
		null,
		null,
		null,
		null,
		DateTime.MinValue,
		0M);
}
