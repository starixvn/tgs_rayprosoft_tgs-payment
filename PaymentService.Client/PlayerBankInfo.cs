using System.Text.Json;

namespace Tgs.Payment.Clients;

public class PlayerBankInfo
{
	public Guid Id { get; init; }

	public string PlayerCode { get; init; } = default!;

	public Guid BrandId { get; init; }

	public Guid PaymentChannelId { get; init; }

	public string BankType { get; init; } = default!;

	public JsonDocument Detail { get; init; } = default!;

	public bool IsDefault { get; init; }

	public DateTime UpdatedOn { get; init; }

	public string? Modifier { get; init; }
}