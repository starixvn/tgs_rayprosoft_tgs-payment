namespace Tgs.Payment
{
	public record BrandPaymentFeeInfo(
		decimal HandlingFeeRateTenPercentile,
		bool ReducePlayerAmount);
}
