namespace Tgs.Payment.Clients;
public record AutoWithdrawInfo
{
	public bool EnableAutoWithdraw { get; set; }
	public decimal AmountUpperLimit { get; set; }
	public string BrandPaymentCandidates { get; set; } = default!;
}
