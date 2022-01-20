namespace Tgs.Payment.Clients;

public class PaymentMethodInfo
{
	public string BankType { get; init; } = default!;

	public Guid Id { get; init; }

	public string Name { get; init; } = default!;
}
