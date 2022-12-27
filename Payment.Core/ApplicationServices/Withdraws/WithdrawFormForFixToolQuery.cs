using System;

namespace Tgs.Payment.ApplicationServices.Withdraws;

public record WithdrawFormForFixToolQuery(
	Guid BrandId,
	string UserId,
	UserType UserType);