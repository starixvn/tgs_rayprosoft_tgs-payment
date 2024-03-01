using System;

namespace Tgs.Payment;
public record PaymentOverview(
	decimal? FirstDepositAmount,
	decimal? FirstDepositHandlingFee,
	DateTime? FirstDepositOn,
	decimal? FirstWithdrawAmount,
	decimal? FirstWithdrawHandlingFee,
	DateTime? FirstWithdrawOn,
	DateOnly RecordedBy,
	decimal TotalDepositAmount,
	int TotalDepositCount,
	decimal TotalWithdrawAmount,
	int TotalWithdrawCount)
{
	public static readonly PaymentOverview Default = new(
		null,
		null,
		null,
		null,
		null,
		null,
		DateOnly.MinValue,
		0M,
		0,
		0M,
		0);
}
