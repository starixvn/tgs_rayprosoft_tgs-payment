using System;

namespace Tgs.Payment.Services
{
	public record SaveWithdrawThirdPartyOrderNoCommand(Guid FormId, string ThirdPartyOrderNo);
}