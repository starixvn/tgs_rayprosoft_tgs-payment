using System;
using System.Collections.Generic;
using System.Linq;

namespace Tgs.Payment.BrandPayment.PaymentConditions;

internal class WhiteListCondition : IBrandPaymentCondition
{
	private readonly Guid[] _whiteList;

	public WhiteListCondition(IEnumerable<Guid> playerAllowBrandPaymentIds)
	{
		_whiteList = playerAllowBrandPaymentIds.ToArray();
	}

	public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount)
		=> _whiteList.Contains(brandPayment.Id);
}
