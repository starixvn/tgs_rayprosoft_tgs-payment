using System;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	public record SubmitDepositFormThirdPartyOrderNoCommand(
		Guid FormId,
		string? ThirdPartyOrderNo);
}