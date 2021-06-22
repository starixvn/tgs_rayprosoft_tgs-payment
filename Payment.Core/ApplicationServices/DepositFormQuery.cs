using System;
using System.Collections.Generic;
using TGS.Paging;

namespace Tgs.Payment.ApplicationServices
{
	public record DepositFormQuery(
		Guid BrandId,
		string? UserName,
		string? ChannelName,
		Guid? PaymentMethodId,
		IEnumerable<PaymentFlowState> Status,
		(DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
		(DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange,
		Paging<DepositFormInfo>? Paging);
}
