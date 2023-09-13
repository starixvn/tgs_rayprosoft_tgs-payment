namespace Tgs.Payment.ViewModels;

public class TopTenDepositWithdrawViewModel
{
	public decimal DepositTotalAmount { get; set; }

	public string PlayerCode { get; set; } = default!;

	public decimal WithdrawTotalAmount { get; set; }
}
