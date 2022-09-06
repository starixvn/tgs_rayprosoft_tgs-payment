namespace Tgs.Payment.Clients;

internal class DepositFormStatus
{
	public decimal? ActualAmount { get; set; }

	public decimal? HandlingFee { get; set; }

	public string? PaymentReturn { get; set; }

	public int? PromotionPlayerId { get; set; }

	public PaymentFlowState? Status { get; set; }

	public string? ThirdPartyOrderNo { get; set; }
}
