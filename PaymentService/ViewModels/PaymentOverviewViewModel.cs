namespace Tgs.Payment.ViewModels;

public class PaymentOverviewViewModel
{
	public decimal? FirstDepositAmount { get; set; }

	public decimal? FirstDepositHandlingFee { get; set; }

	public DateTime? FirstDepositOn { get; set; }

	public decimal? FirstWithdrawAmount { get; set; }

	public decimal? FirstWithdrawHandlingFee { get; set; }

	public DateTime? FirstWithdrawOn { get; set; }

	public required DateOnly RecordedBy { get; set; }

	public required decimal TotalDepositAmount { get; set; }

	public required int TotalDepositCount { get; set; }

	public required decimal TotalWithdrawAmount { get; set; }

	public required int TotalWithdrawCount { get; set; }
}
