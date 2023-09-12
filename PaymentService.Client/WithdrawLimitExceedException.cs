namespace Tgs.Payment.Clients;

public class WithdrawLimitExceedException : Exception
{
	public decimal? Amount { get; }

	public int? Count { get; }

	public WithdrawLimitExceedException(int? count, decimal? amount)
	{
		Count = count;
		Amount = amount;
	}
}
