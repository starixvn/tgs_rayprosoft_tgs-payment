using System;
using System.Collections.Generic;
using TGS.Paging;

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
	(DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
	(DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange,
	(DateTimeOffset Begin, DateTimeOffset End)? PaidOnRange,
	(DateTimeOffset Begin, DateTimeOffset End)? ReviewOnRange,
	Paging<WithdrawFormInfo> Paging);
