namespace Tgs.Payment.Clients;
public record PaymentOverview(
	decimal? FirstDepositAmount,
	decimal? FirstDepositHandlingFee,
	DateTime? FirstDepositOn,
	decimal? FirstWithdrawAmount,
	decimal? FirstWithdrawHandlingFee,
	DateTime? FirstWithdrawOn,
	DateOnly RecordedBy,
	decimal TotalDepositAmount,
	decimal TotalWithdrawAmount);
