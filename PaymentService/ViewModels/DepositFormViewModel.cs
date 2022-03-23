namespace Tgs.Payment.ViewModels;

public class DepositFormViewModel
{
	public decimal? ActualAmount { get; set; }

	public decimal Amount { get; set; }

	public string? ApplyInfo { get; set; }

	public Guid BrandId { get; set; }

	public Guid BrandPaymentId { get; set; }

	public string ChannelName { get; set; } = default!;

	public DateTimeOffset CreatedOn { get; set; }

	public Guid Id { get; set; }

	public string? OrderNo { get; set; }

	public Guid PaymentMethodId { get; set; }

	public string? PaymentReturn { get; set; }

	public string PlayerCode { get; set; } = default!;

	public Guid? PromotionId { get; set; }

	public int? PromotionPlayerId { get; set; }

	public string? Remark { get; set; }

	public string? ReviewedUserName { get; set; }

	public PaymentFlowState Status { get; set; }

	public string? ThirdPartyOrderNo { get; set; }

	public DateTimeOffset UpdatedOn { get; set; }

	public string UserName { get; set; } = default!;
}
