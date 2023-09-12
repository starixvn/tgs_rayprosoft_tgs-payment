using System;
using System.Collections.Generic;

namespace Tgs.Payment;

public class WithdrawLimitExceedException : Exception
{
	private readonly IReadOnlyDictionary<string, object> _errors;

	public decimal? Amount => (decimal?)_errors.GetValueOrDefault("Amount", null!);

	public int? Count => (int?)_errors.GetValueOrDefault("Count", null!);

	public WithdrawLimitExceedException(IReadOnlyDictionary<string, object> errors)
	{
		_errors = errors ?? throw new ArgumentNullException(nameof(errors));
	}
}
