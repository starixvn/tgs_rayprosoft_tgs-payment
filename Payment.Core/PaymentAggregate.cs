using System;

namespace Tgs.Payment;

class PaymentAggregate
{
	public decimal? DepositAmount { get; set; }

	public decimal? DepositHandlingFee { get; set; }

	public int? DepositCount { get; set; }

	public decimal? FirstDepositAmount { get; set; }

	public decimal? FirstDepositHandlingFee { get; set; }

	public DateTime? FirstDepositOn { get; set; }

	public decimal? WithdrawAmount { get; set; }

	public decimal? WithdrawHandlingFee { get; set; }

	public int? WithdrawCount { get; set; }

	public decimal? FirstWithdrawAmount { get; set; }

	public decimal? FirstWithdrawHandlingFee { get; set; }

	public DateTime? FirstWithdrawOn { get; set; }
}
