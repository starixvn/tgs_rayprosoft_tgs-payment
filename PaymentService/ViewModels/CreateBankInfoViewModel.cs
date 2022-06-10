using System.Text.Json;

namespace Tgs.Payment.ViewModels;

public class CreateBankInfoViewModel
{
	public string BankType { get; set; } = default!;

	public JsonDocument Detail { get; set; } = default!;

	public bool? IsDefault { get; set; }

	public Guid PaymentChannelId { get; set; }
}
