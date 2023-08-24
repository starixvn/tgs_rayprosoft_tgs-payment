using System;
using System.Collections.Generic;

namespace Tgs.Payment;
public record AutoWithdrawInfo(
	bool EnableAutoWithdraw,
	decimal AmountUpperLimit,
	IEnumerable<Guid> BrandPaymentIds);
