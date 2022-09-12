namespace Tgs.Payment.Clients;

public record BrandPaymentFeeInfo
{
	public Guid BrandPaymentId { get; init; }
	public decimal HandlingFeeRateTenPercentile { get; init; }
	public bool ReducePlayerAmount { get; init; }
}
