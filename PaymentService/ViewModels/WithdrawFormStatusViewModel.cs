namespace Tgs.Payment.ViewModels;

public class WithdrawFormStatusViewModel
{
	public decimal? ActualAmount { get; set; }

	public string? PaidUserId { get; set; }

	public string? PaidUserName { get; set; }

	public string? PaymentReturn { get; set; }

	public PaymentFlowState? Status { get; set; }

	public string? ThirdPartyOrderNo { get; set; }
}
