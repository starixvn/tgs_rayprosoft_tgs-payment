namespace Tgs.Payment.Clients;
public record AutoWithdrawInfo
{
	public bool EnableAutoWithdraw { get; set; }
	public decimal AmountUpperLimit { get; set; }
	public IEnumerable<Guid> BrandPaymentIds { get; set; } = default!;
}
