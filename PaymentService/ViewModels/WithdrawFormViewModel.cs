namespace Tgs.Payment.ViewModels;

public class WithdrawFormViewModel
{
	public decimal? ActualAmount { get; set; }

	public decimal Amount { get; set; }

	public string? ApplyInfo { get; set; }

	public string? BankInfo { get; set; }

	public Guid BankInfoId { get; set; }

	public Guid BrandId { get; set; }

	public Guid? BrandPaymentId { get; set; }

	public string? ChannelName { get; set; }

	public DateTimeOffset CreatedOn { get; set; }

	public decimal HandlingFee { get; set; }

	public Guid Id { get; set; }

	public string? Note { get; set; }

	public string? OrderNo { get; set; }

	public DateTimeOffset? PaidOn { get; set; }

	public string? PaidUserId { get; set; }

	public string? PaidUserName { get; set; }

	public Guid? PaymentMethodId { get; set; }

	public string? PaymentReturn { get; set; }

	public string? Remark { get; set; }

	public DateTimeOffset? ReviewedOn { get; set; }

	public string? ReviewedUserName { get; set; }

	public PaymentFlowState Status { get; set; }

	public string? ThirdPartyOrderNo { get; set; }

	public DateTimeOffset UpdatedOn { get; set; }

	public string? UserId { get; set; }

	public string? UserName { get; set; }

	public UserType UserType { get; set; }
}
