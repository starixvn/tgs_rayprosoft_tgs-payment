using System;
using System.Collections.Generic;
using Tgs.Paging;

namespace Tgs.Payment.ApplicationServices.Withdraws;

public record WithdrawFormQuery(
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
	IEnumerable<Guid> BrandPaymentIds,
	(DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
	(DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange,
	(DateTimeOffset Begin, DateTimeOffset End)? PaidOnRange,
	(DateTimeOffset Begin, DateTimeOffset End)? ReviewOnRange,
	Paging<WithdrawFormInfo> Paging);
