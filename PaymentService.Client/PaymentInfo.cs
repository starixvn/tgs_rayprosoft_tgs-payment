namespace Tgs.Payment.Clients;

public class PaymentInfo
{
	public string Code { get; init; } = default!;

	public Guid Id { get; init; }

	public string Description { get; init; } = default!;
}
