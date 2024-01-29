using System;

namespace Tgs.Payment;
public record UserPaymentSummaryStatistics(
	DateTime TimePoint,
	string UserId,
	UserType UserType,
	decimal? DepositAmount,
	decimal? DepositHandlingFee,
	int? DepositCount,
	decimal? FirstDepositAmount,
	decimal? FirstDepositHandlingFee,
	DateTime? FirstDepositOn,
	decimal? WithdrawAmount,
	decimal? WithdrawHandlingFee,
	int? WithdrawCount,
	decimal? FirstWithdrawAmount,
	decimal? FirstWithdrawHandlingFee,
	DateTime? FirstWithdrawOn);
