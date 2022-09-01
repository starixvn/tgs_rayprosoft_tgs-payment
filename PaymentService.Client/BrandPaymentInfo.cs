namespace Tgs.Payment.Clients;

public record BrandPaymentInfo
{
	public bool AuthenticateIdentity { get; init; }

	public bool AuthenticatePhone { get; init; }

	public string ChannelCode { get; init; } = default!;

	public Guid ChannelId { get; init; }

	public string ChannelName { get; init; } = default!;

	public ConditionMatchType ConditionMatchType { get; init; }

	public ValueRange<decimal> DepositedAmountLimit { get; init; }

	public bool Enabled { get; init; }

	public string? ExtendedData { get; init; }

	public Guid Id { get; init; }

	public bool IsAllPlayers { get; init; }

	public Guid MethodId { get; init; }

	public Guid PaymentId { get; init; }

	public Guid PaymentMethodChannelId { get; init; }

	public ValueRange<decimal> TransferLimit { get; init; }

	public TransferType TransferType { get; init; }

	public int DisplayOrder { get; init; }
	public string PaymentName { get; init; } = default!;

	public string PaymentMethodName { get; init; } = default!;

	public IEnumerable<Guid> VipLevelIds { get; init; } = default!;
}
