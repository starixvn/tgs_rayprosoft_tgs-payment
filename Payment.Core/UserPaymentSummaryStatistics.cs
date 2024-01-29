using System;

namespace Tgs.Payment;
public record UserPaymentSummaryStatistics(
	DateTime TimePoint,
	string UserId,
	UserType UserType,
	decimal? DepositAmount,
	decimal? DepositHandlingFee,
	int? DepositCount,
	decimal? WithdrawAmount,
	decimal? WithdrawHandlingFee,
	int? WithdrawCount);
