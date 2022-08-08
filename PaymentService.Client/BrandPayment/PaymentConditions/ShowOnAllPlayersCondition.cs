namespace Tgs.Payment.Clients.BrandPayment.PaymentConditions;

internal class ShowOnAllPlayersCondition : IBrandPaymentCondition
{
	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player)
		=> brandPayment.IsAllPlayers;
}
