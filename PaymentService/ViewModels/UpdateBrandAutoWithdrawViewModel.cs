using System.Text.Json;

namespace Tgs.Payment.ViewModels;

public class UpdateBrandAutoWithdrawViewModel
{
	public decimal AmountUpperLimit { get; set; }

	public JsonDocument BrandPaymentCandidates { get; set; } = default!;

	public bool EnableAutoWitdraw { get; set; }

	public string ModifyUserId { get; set; } = default!;
}
