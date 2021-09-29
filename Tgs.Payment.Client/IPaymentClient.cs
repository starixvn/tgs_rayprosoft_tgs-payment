using System;
using System.Threading.Tasks;

namespace Tgs.Payment.Clients
{
	public interface IPaymentClient
	{
		ValueTask ApproveDepositAsync(Guid formId, Guid brandId);

		ValueTask<string> AskAsync(string subject, string requestInfo);

		ValueTask<string> DepositAsync(string subject, decimal amount, string? applyInfo);

		ValueTask EmitDepositFormCreatedEventAsync(Guid formId, Guid brandId);

		ValueTask EmitDepositFormStatusChangedEventAsync(Guid formId, Guid brandId);

		ValueTask EmitWithdrawFormCreatedEventAsync(Guid fromId, Guid brandId);

		ValueTask EmitWithdrawFormStatusChangedEventAsync(Guid formId, Guid brandId);

		ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo);

		ValueTask EmitWithdrawFormThirdPartyOrderNoAsync(Guid formId, string? thirdPartyOrderNo);
	}
}