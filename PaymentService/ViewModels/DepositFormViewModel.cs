namespace Tgs.Payment.ViewModels;

public class DepositFormViewModel
{
	public decimal? ActualAmount { get; set; }

	public required decimal Amount { get; set; }

	public string? ApplyInfo { get; set; }

	public required Guid BrandId { get; set; }

	public required Guid BrandPaymentId { get; set; }

	public required string ChannelName { get; set; }

	public required DateTimeOffset CreatedOn { get; set; }

	public required decimal HandlingFee { get; set; }

	public required Guid Id { get; set; }

	public string? OrderNo { get; set; }

	public required Guid PaymentMethodId { get; set; }

	public string? PaymentReturn { get; set; }

	public required string PlayerCode { get; set; }

	public Guid? PromotionId { get; set; }

	public int? PromotionPlayerId { get; set; }

	public string? Remark { get; set; }

	public DateTimeOffset? ReviewedOn { get; set; }

	public string? ReviewedUserName { get; set; }

	public required PaymentFlowState Status { get; set; }

	public string? ThirdPartyOrderNo { get; set; }

	public required DateTimeOffset UpdatedOn { get; set; }

	public required string UserName { get; set; }

	public required decimal ApplyAmount { get; set; }
}
