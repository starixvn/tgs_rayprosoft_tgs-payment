namespace Tgs.Payment.Repositories;

public class TopTenInfo
{
	public decimal DepositTotalAmount { get; set; }

	public string PlayerCode { get; set; } = default!;

	public decimal WithdrawTotalAmount { get; set; }
};
