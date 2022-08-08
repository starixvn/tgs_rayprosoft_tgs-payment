namespace Tgs.Payment.Clients.BrandPayment.PaymentConditions;

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

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player)
	{
		if (!_blockListCondition.IsValid(brandPayment, player))
			return false;

		if (_whiteListCondition.IsValid(brandPayment, player))
			return true;

		if (!_showOnAllPlayersCondition.IsValid(brandPayment, player))
			return false;

		return _brandPaymentCondition.IsValid(brandPayment, player);
	}
}
