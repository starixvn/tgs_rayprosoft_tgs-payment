namespace Tgs.Payment.ViewModels;

public class PaymentStatisticsDeltaViewModel
{
	public required Guid Id { get; set; }

	public required DateTime TimePoint { get; set; }

	public required string UserId { get; set; }

	public required decimal DepositAmount { get; set; }

	public required decimal DepositHandlingFee { get; set; }

	public required int DepositCount { get; set; }

	public required decimal WithdrawAmount { get; set; }

	public required decimal WithdrawHandlingFee { get; set; }

	public required int WithdrawCount { get; set; }
}
