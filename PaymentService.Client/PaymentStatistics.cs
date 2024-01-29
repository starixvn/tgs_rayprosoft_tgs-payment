namespace Tgs.Payment.Clients;

public record PaymentStatistics(
	DateTime TimePoint,
	string UserId,
	decimal? DepositAmount,
	decimal? DepositHandlingFee,
	int? DepositCount,
	decimal? WithdrawAmount,
	decimal? WithdrawHandlingFee,
	int? WithdrawCount);
