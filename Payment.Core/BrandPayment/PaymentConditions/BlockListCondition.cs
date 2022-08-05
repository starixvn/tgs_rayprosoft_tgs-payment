using System;
using System.Collections.Generic;
using System.Linq;

namespace Tgs.Payment.BrandPayment.PaymentConditions;

internal class BlockListCondition : IBrandPaymentCondition
{
    private Guid[] _blockList;

    public BlockListCondition(IEnumerable<Guid> playerBlockedBrandPaymentIds)
    {
        _blockList = playerBlockedBrandPaymentIds.ToArray();
    }

    public bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount)
        => !_blockList.Contains(brandPayment.Id);
}
