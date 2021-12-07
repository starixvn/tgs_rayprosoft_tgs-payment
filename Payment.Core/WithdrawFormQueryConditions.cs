using System;
using System.Collections.Generic;

namespace Tgs.Payment
{
	public record WithdrawFormQueryConditions(
		Guid BrandId,
		string? UserId,
		string? UserName,
		UserType? UserType,
		string? ChannelName,
		string? ThirdPartyOrderNo,
		string? OrderNo,
		Guid? FormId,
		Guid? PaymentMethodId,
		IEnumerable<PaymentFlowState> Status,
		(DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
		(DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange,
		(DateTimeOffset Begin, DateTimeOffset End)? ReviewOnRange,
		(DateTimeOffset Begin, DateTimeOffset End)? PaidOnRange);
}