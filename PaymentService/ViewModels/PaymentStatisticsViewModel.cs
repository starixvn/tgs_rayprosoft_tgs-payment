namespace Tgs.Payment.ViewModels;

public class PaymentStatisticsViewModel
{
	public required DateTime TimePoint { get; set; }

	public required string UserId { get; set; }

	public decimal? DepositAmount { get; set; }

	public decimal? DepositHandlingFee { get; set; }

	public int? DepositCount { get; set; }

	public decimal? WithdrawAmount { get; set; }

	public decimal? WithdrawHandlingFee { get; set; }

	public int? WithdrawCount { get; set; }
}
