using System.Text.Json;

namespace Tgs.Payment.ViewModels;

public class AgentBankInfoViewModel
{
	public Guid AgentId { get; set; } = default!;

	public string BankType { get; set; } = default!;

	public Guid BrandId { get; set; }

	public JsonDocument Detail { get; set; } = default!;

	public Guid Id { get; set; }

	public Guid PaymentChannelId { get; set; }
}
