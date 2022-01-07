namespace Tgs.Payment.Clients;

public class BrandPaymentAcl
{
	public Guid BrandPaymentId { get; init; }

	public bool IsBlock { get; init; }

	public string PlayerCode { get; init; } = default!;
}
