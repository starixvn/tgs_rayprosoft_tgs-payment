namespace Tgs.Payment
{
	public record BrandPaymentFeeInfo(
		decimal HandlingFeeRatePercentile,
		bool ReducePlayerAmount);
}
