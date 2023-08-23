namespace Tgs.Payment.ViewModels;

public class BrandAutoWithdrawViewModel
{
	public decimal AmountUpperLimit { get; set; }

	public string BrandPaymentCandidates { get; set; } = default!;

	public bool EnableAutoWithdraw { get; set; }
}
