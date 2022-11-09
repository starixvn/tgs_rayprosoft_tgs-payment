using System.Text.Json;

namespace Tgs.Payment.ViewModels;

public class UpdateBrandPaymentExtendedDataViewModel
{
	public JsonDocument ExtendedData { get; set; } = default!;
}
