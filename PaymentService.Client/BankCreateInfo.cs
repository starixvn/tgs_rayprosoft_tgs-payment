using System.Text.Json;

namespace Tgs.Payment.Clients;

public class BankCreateInfo
{
	public string BankType { get; init; } = default!;

	public JsonDocument Detail { get; init; } = default!;

	public Guid PaymentChannelId { get; init; }
}
