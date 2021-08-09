using System.Text.Json;

namespace Tgs.Payment.ViewModels
{
	public class PaymentCreateViewModel
	{
		public string Code { get; set; } = default!;

		public string? Description { get; set; }

		public JsonDocument? ExtendedData { get; set; }
	}
}
