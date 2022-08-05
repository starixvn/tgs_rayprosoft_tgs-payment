using System.Linq;

namespace Tgs.Payment.BrandPayment.PaymentConditions;

internal class MatchAllCondition : IBrandPaymentCondition
{
	private readonly IBrandPaymentCondition[] _brandPaymentConditions;

	public MatchAllCondition(params IBrandPaymentCondition[] brandPaymentConditions)
	{
		_brandPaymentConditions = brandPaymentConditions;
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount)
		=> _brandPaymentConditions.All(condition => condition.IsValid(
			brandPayment,
			player,
			depositedAmount));
}
