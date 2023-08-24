using System;
using System.Collections.Generic;

namespace Tgs.Payment.ApplicationServices.BrandPayments;
public record UpdateBrandAutoWithdrawCommand(
	Guid BrandId,
	bool EnableAutoWithdraw,
	decimal AmountUpperLimit,
	IEnumerable<Guid> BrandPaymentIds,
	string UserId);
