using System;
using System.Collections.Generic;

namespace Tgs.Payment
{
	public record DepositFormQueryConditions(
		Guid BrandId,
		string? UserName,
		string? ChannelName,
		string? ThirdPartyOrderNo,
		Guid? FormId,
		Guid? PaymentMethodId,
		IEnumerable<PaymentFlowState> Status,
		(DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
		(DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange);
}