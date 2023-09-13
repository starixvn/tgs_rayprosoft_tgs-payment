namespace Tgs.Payment;
public record TopTenDepositWithdrawInfo
{
	public string PlayerCode { get; set; } = default!;

	public decimal DepositTotalAmount { get; set; }

	public decimal WithdrawTotalAmount { get; set; }
}
