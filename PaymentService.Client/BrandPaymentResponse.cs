using System.Text.Json;

namespace Tgs.Payment.Clients;

public record BrandPaymentResponse
{
	public string BankType { get; init; } = default!;

	public string ChannelCode { get; init; } = default!;

	public Guid ChannelId { get; init; }

	public string ChannelName { get; init; } = default!;

	public int DisplayOrder { get; init; }

	public JsonDocument ExtendedData { get; init; } = default!;

	public Guid Id { get; init; }

	public string MethodCode { get; init; } = default!;

	public Guid MethodId { get; init; }

	public string PaymentCode { get; init; } = default!;

	public Guid PaymentId { get; init; }

	public ValueRange<decimal> TransferLimit { get; init; }
}
