using System;
using System.Collections.Generic;
using TGS.Paging;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	public record DepositFormQuery(
		Guid BrandId,
		string? UserId,
		string? UserName,
		string? ChannelName,
		string? ThirdPartyOrderNo,
		string? OrderNo,
		Guid? FormId,
		Guid? PaymentMethodId,
		IEnumerable<PaymentFlowState> Status,
		(DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
		(DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange,
		Paging<DepositFormInfo> Paging);
}
