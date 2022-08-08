namespace Tgs.Payment.Clients.BrandPayment.PaymentConditions;

internal class MatchAllCondition : IBrandPaymentCondition
{
	private readonly IBrandPaymentCondition[] _brandPaymentConditions;

	public MatchAllCondition(params IBrandPaymentCondition[] brandPaymentConditions)
	{
		_brandPaymentConditions = brandPaymentConditions;
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player)
		=> _brandPaymentConditions.All(condition => condition.IsValid(
			brandPayment,
			player));
}
