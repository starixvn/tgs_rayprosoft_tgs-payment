using System;
using System.Collections.Generic;

namespace Tgs.Payment;

public class WithdrawLimitExceedException(IReadOnlyDictionary<string, object> errors) : Exception
{
	public decimal? Amount => (decimal?)errors.GetValueOrDefault("Amount", null!);

	public int? Count => (int?)errors.GetValueOrDefault("Count", null!);
}
