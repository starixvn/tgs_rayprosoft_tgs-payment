using System.Text.Json;

namespace Tgs.Payment.Clients
{
	public class PaymentCreateInfo
    {
        public string Code { get; init; } = default!;

        public string? Description { get; init; }

        public JsonDocument? ExtendedData { get; init; }
    }
}