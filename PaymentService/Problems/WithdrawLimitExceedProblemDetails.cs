using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.Problems;

public class WithdrawLimitExceedProblemDetails : ProblemDetails
{
	public int? Count { get; set; }
	public decimal? Amount { get; set; }
}
