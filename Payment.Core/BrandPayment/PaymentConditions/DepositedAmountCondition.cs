namespace Tgs.Payment.BrandPayment.PaymentConditions;

internal class DepositedAmountCondition : IBrandPaymentCondition
{
	private ValueRange<decimal> _depositedAmountLimit;

	public DepositedAmountCondition(ValueRange<decimal> depositedAmountLimit)
	{
		_depositedAmountLimit = depositedAmountLimit;
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount)
		=> depositedAmount >= _depositedAmountLimit.From
			&& depositedAmount < _depositedAmountLimit.To;
}
