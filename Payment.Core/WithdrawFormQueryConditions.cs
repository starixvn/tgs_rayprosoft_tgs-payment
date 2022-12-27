using System;
using System.Collections.Generic;

namespace Tgs.Payment;

public record WithdrawFormQueryConditions(
	Guid BrandId,
	string? UserId,
	string? UserName,
	UserType? UserType,
	string? ChannelName,
	string? ThirdPartyOrderNo,
	string? OrderNo,
	Guid? FormId,
	Guid? PaymentId,
	Guid? PaymentMethodId,
	IEnumerable<PaymentFlowState> Status,
	IEnumerable<Guid> BrandPaymentIds,
	(DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
	(DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange,
	(DateTimeOffset Begin, DateTimeOffset End)? ReviewOnRange,
	(DateTimeOffset Begin, DateTimeOffset End)? PaidOnRange)
{
	public static readonly WithdrawFormQueryConditions Default = new(
		Guid.Empty,
		null,
		null,
		null,
		null,
		null,
		null,
		null,
		null,
		null,
		Array.Empty<PaymentFlowState>(),
		Array.Empty<Guid>(),
		null,
		null,
		null,
		null);
};
