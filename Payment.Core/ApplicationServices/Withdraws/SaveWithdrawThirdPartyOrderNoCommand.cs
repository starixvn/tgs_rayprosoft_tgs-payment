using System;

namespace Tgs.Payment.ApplicationServices.Withdraws
{
	public record SaveWithdrawThirdPartyOrderNoCommand(Guid FormId, string ThirdPartyOrderNo);
}
