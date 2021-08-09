using System.Text.Json;

namespace Tgs.Payment.Clients
{
	public class PaymentCreateInfo
	{
		public string Code { get; set; } = default!;

		public string? Description { get; set; }

		public JsonDocument? ExtendedData { get; set; }
	}
}
