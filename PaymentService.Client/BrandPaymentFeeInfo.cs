namespace Tgs.Payment.Clients
{
	public record BrandPaymentFeeInfo
	{
		public decimal HandlingFeeRatePercentile { get; init; }
		public bool ReducePlayerAmount { get; init; }
	}
}
