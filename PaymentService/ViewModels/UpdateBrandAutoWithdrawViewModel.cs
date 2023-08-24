namespace Tgs.Payment.ViewModels;

public class UpdateBrandAutoWithdrawViewModel
{
	public decimal AmountUpperLimit { get; set; }

	public IEnumerable<Guid> BrandPaymentIds { get; set; } = default!;

	public bool EnableAutoWitdraw { get; set; }
}
