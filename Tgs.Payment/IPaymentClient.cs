using System.Threading.Tasks;

namespace Tgs.Payment
{
	public interface IPaymentClient
	{
		ValueTask<string> DepositAsync(string subject, decimal amount, string? applyInfo);

		ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo);
	}
}
