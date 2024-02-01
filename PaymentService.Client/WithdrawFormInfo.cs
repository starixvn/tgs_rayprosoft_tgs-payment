namespace Tgs.Payment.Clients;

public class WithdrawFormInfo
{
	public decimal? ActualAmount { get; init; }

	public decimal Amount { get; init; }

	public string? ApplyInfo { get; init; }

	public string? BankInfo { get; init; }

	public Guid BankInfoId { get; init; }

	public Guid BrandId { get; init; }

	public Guid? BrandPaymentId { get; init; }

	public string? ChannelName { get; init; }

	public DateTimeOffset CreatedOn { get; init; }

	public decimal HandlingFee { get; init; }

	public Guid Id { get; init; }

	public string? Note { get; init; }

	public string? OrderNo { get; init; }

	public DateTimeOffset? PaidOn { get; init; }

	public string? PaidUserId { get; init; }

	public string? PaidUserName { get; init; }

	public Guid? PaymentMethodId { get; init; }

	public string? PaymentReturn { get; init; }

	public string? Remark { get; init; }

	public string? PlayerRemark { get; init; }

	public DateTimeOffset? ReviewedOn { get; init; }

	public string? ReviewedUserName { get; init; }

	public PaymentFlowState Status { get; init; }

	public string? ThirdPartyOrderNo { get; init; }

	public DateTimeOffset UpdatedOn { get; init; }

	public string? UserId { get; init; }

	public string? UserName { get; init; }

	public UserType UserType { get; init; }
}
