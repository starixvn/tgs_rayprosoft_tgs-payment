using System;
using System.Threading.Tasks;

namespace Tgs.Payment.Clients
{
	public interface IPaymentClient
	{
		ValueTask<string> DepositAsync(string subject, decimal amount, string? applyInfo);

		ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo);

		ValueTask<string> AskAsync(string subject, string requestInfo);

		ValueTask ApproveDepositAsync(Guid formId, Guid brandId);
	}
}
