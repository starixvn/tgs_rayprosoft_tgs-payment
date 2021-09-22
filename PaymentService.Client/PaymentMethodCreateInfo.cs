namespace Tgs.Payment.Clients
{
	public class PaymentMethodCreateInfo
    {
        public string BankType { get; init; } = default!;

        public string Code { get; init; } = default!;
    }
}