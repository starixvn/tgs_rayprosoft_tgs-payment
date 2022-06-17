using System.Text.Json;

namespace Tgs.Payment.ViewModels;

public class PlayerBankInfoViewModel
{
	public string BankType { get; set; } = default!;

	public Guid BrandId { get; set; }

	public JsonDocument Detail { get; set; } = default!;

	public Guid Id { get; set; }

	public bool IsDefault { get; set; }

	public Guid PaymentChannelId { get; set; }

	public string PlayerCode { get; set; } = default!;
}
