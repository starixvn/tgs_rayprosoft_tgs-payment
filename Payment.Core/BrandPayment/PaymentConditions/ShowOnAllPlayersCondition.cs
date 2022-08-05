namespace Tgs.Payment.BrandPayment.PaymentConditions;

internal class ShowOnAllPlayersCondition : IBrandPaymentCondition
{
	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount)
		=> brandPayment.IsAllPlayers;
}
