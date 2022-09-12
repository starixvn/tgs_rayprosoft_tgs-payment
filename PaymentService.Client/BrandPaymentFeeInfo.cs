namespace Tgs.Payment.Clients;

public record BrandPaymentFeeInfo
{
	public decimal HandlingFeeRateTenPercentile { get; init; }
	public bool ReducePlayerAmount { get; init; }
	public Guid BrandPaymentId { get; init; }
}
