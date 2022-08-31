namespace Tgs.Payment.ViewModels;

public class BrandPaymentViewModel
{
	public bool AuthenticateIdentity { get; set; }

	public bool AuthenticatePhone { get; set; }

	public string ChannelCode { get; set; } = default!;

	public Guid ChannelId { get; set; }

	public string ChannelName { get; set; } = default!;

	public ConditionMatchType ConditionMatchType { get; set; }

	public ValueRange<decimal> DepositedAmountLimit { get; set; }

	public int DisplayOrder { get; set; }

	public bool Enabled { get; set; }

	public string? ExtendedData { get; set; }

	public decimal HandlingFeeRateTenPercentile { get; init; }

	public Guid Id { get; set; }

	public bool IsAllPlayers { get; set; }

	public Guid MethodId { get; set; }

	public Guid PaymentId { get; set; }

	public Guid PaymentMethodChannelId { get; set; }

	public bool ReducePlayerAmount { get; init; }

	public ValueRange<decimal> TransferLimit { get; set; }

	public TransferType TransferType { get; set; }
}
