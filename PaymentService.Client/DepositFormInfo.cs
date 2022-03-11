namespace Tgs.Payment.Clients;

public class DepositFormInfo
{
	public decimal? ActualAmount { get; init; }

	public decimal Amount { get; init; }

	public string? ApplyInfo { get; init; }

	public Guid BrandId { get; init; }

	public Guid BrandPaymentId { get; init; }

	public string ChannelName { get; init; } = default!;

	public DateTimeOffset CreatedOn { get; init; }

	public Guid Id { get; init; }

	public string? OrderNo { get; init; }

	public Guid PaymentMethodId { get; init; }

	public string? PaymentReturn { get; init; }

	public string PlayerCode { get; init; } = default!;

	public Guid? PromotionId { get; init; }

	public int? PromotionPlayerId { get; init; }

	public string? Remark { get; init; }

	public string? ReviewedUserName { get; init; }

	public PaymentFlowState Status { get; init; }

	public string? ThirdPartyOrderNo { get; init; }

	public DateTimeOffset UpdatedOn { get; init; }

	public string UserName { get; init; } = default!;
}
