namespace Tgs.Payment.ViewModels;

public class BrandPaymentFeeViewModel
{
	public Guid BrandPaymentId { get; init; }

	public decimal HandlingFeeRateTenPercentile { get; init; }

	public bool ReducePlayerAmount { get; init; }
}
