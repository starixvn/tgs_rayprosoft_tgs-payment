namespace Tgs.Payment.ViewModels;

public class BrandAutoWithdrawViewModel
{
	public decimal AmountUpperLimit { get; set; }

	public IEnumerable<Guid> BrandPaymentIds { get; set; } = default!;

	public bool EnableAutoWithdraw { get; set; }
}
