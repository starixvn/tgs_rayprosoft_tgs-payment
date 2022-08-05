namespace Tgs.Payment.BrandPayment.PaymentConditions;

internal class PhoneCondition : IBrandPaymentCondition
{
	private readonly bool _enabled;

	public PhoneCondition(bool enabled)
	{
		_enabled = enabled;
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount)
		=> _enabled ? player.PhoneAuthentication : true;
}
