namespace Tgs.Payment.Clients;

public class TopTenDepositWithdrawInfo
{
	public decimal DepositTotalAmount { get; set; }

	public string PlayerCode { get; set; } = default!;

	public decimal WithdrawTotalAmount { get; set; }
}
