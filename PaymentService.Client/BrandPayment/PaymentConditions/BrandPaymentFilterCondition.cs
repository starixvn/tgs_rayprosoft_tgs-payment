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
		_blockListCondition = blockListCondition ?? throw new ArgumentNullException(nameof(blockListCondition));
		_whiteListCondition = whiteListCondition ?? throw new ArgumentNullException(nameof(whiteListCondition));
		_showOnAllPlayersCondition = showOnAllPlayersCondition ?? throw new ArgumentNullException(nameof(showOnAllPlayersCondition));
		_brandPaymentCondition = brandPaymentCondition ?? throw new ArgumentNullException(nameof(brandPaymentCondition));
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player)
		=> _blockListCondition.IsValid(brandPayment, player)
			&& (_whiteListCondition.IsValid(brandPayment, player)
				|| _showOnAllPlayersCondition.IsValid(brandPayment, player)
				&& _brandPaymentCondition.IsValid(brandPayment, player));
}
