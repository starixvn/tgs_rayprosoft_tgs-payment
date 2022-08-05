namespace Tgs.Payment.BrandPayment.PaymentConditions;

internal class IdentityCondition : IBrandPaymentCondition
{
	private readonly bool _enabled;

	public IdentityCondition(bool enabled)
	{
		_enabled = enabled;
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount)
		=> _enabled ? player.IdentityAuthentication : true;
}
