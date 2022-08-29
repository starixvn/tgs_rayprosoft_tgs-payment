namespace Tgs.Payment.Clients.BrandPayment.PaymentConditions;

internal class VipCondition : IBrandPaymentCondition
{
	private readonly IEnumerable<Guid> _allowVips;

	public VipCondition(IEnumerable<Guid> allowVips)
	{
		_allowVips = allowVips.ToArray();
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player)
		=> player.VipLevelId != null && _allowVips.Contains(player.VipLevelId.Value);
}
