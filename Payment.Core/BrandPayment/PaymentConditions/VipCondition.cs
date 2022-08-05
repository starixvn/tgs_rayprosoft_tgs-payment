using System;
using System.Collections.Generic;
using System.Linq;

namespace Tgs.Payment.BrandPayment.PaymentConditions;

internal class VipCondition : IBrandPaymentCondition
{
	private readonly IEnumerable<Guid> _allowVips;

	public VipCondition(IEnumerable<Guid> allowVips)
	{
		_allowVips = allowVips.ToArray();
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount)
	{
		if (player.VipLevelId == null)
			return false;

		return _allowVips.Contains(player.VipLevelId.Value);
	}
}
