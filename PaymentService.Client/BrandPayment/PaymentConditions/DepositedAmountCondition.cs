namespace Tgs.Payment.Clients.BrandPayment.PaymentConditions;

internal class DepositedAmountCondition : IBrandPaymentCondition
{
	private readonly ValueRange<decimal> _depositedAmountLimit;

	public DepositedAmountCondition(ValueRange<decimal> depositedAmountLimit)
	{
		_depositedAmountLimit = depositedAmountLimit;
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player)
		=> player.DepositedAmount >= _depositedAmountLimit.From
			&& player.DepositedAmount < _depositedAmountLimit.To;
}
