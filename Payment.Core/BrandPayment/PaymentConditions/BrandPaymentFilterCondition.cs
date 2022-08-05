namespace Tgs.Payment.BrandPayment.PaymentConditions;

internal class BrandPaymentFilterCondition : IBrandPaymentCondition
{
	private readonly BlockListCondition _blockListCondition;
	private readonly IBrandPaymentCondition _brandPaymentCondition;
	private readonly ShowOnAllPlayersCondition _showOnAllPlayersCondition;
	private readonly WhiteListCondition _whiteListCondition;

	public BrandPaymentFilterCondition(
		BlockListCondition blockListCondition,
		WhiteListCondition whiteListCondition,
		ShowOnAllPlayersCondition showOnAllPlayersCondition,
		IBrandPaymentCondition brandPaymentCondition)
	{
		_blockListCondition = blockListCondition;
		_whiteListCondition = whiteListCondition;
		_showOnAllPlayersCondition = showOnAllPlayersCondition;
		_brandPaymentCondition = brandPaymentCondition;
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount)
	{
		if (!_blockListCondition.IsValid(brandPayment, player, depositedAmount))
			return false;

		if (_whiteListCondition.IsValid(brandPayment, player, depositedAmount))
			return true;

		if (!_showOnAllPlayersCondition.IsValid(brandPayment, player, depositedAmount))
			return false;

		return _brandPaymentCondition.IsValid(brandPayment, player, depositedAmount);
	}
}
