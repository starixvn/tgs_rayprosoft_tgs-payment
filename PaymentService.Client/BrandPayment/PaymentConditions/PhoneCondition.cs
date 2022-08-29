namespace Tgs.Payment.Clients.BrandPayment.PaymentConditions;

internal class PhoneCondition : IBrandPaymentCondition
{
	private readonly bool _enabled;

	public PhoneCondition(bool enabled)
	{
		_enabled = enabled;
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player)
		=> !_enabled || player.PhoneAuthentication;
}
