using System;
using System.Threading.Tasks;

namespace Tgs.Payment.Clients
{
	public interface IPaymentClient
	{
		ValueTask ApproveDepositAsync(Guid formId, Guid brandId, decimal? actualAmount);

		ValueTask<string> AskAsync(string subject, string requestInfo);

		ValueTask<string> DepositAsync(string subject, decimal amount, string? applyInfo);

		ValueTask EmitDepositFormCreatedEventAsync(Guid formId, Guid brandId);

		ValueTask EmitDepositFormStatusChangedEventAsync(Guid formId, Guid brandId, string? status);

		ValueTask EmitWithdrawFormCreatedEventAsync(Guid fromId, Guid brandId);

		ValueTask EmitWithdrawFormStatusChangedEventAsync(Guid formId, Guid brandId, string? status);

		ValueTask SaveWithdrawFormThirdPartyOrderNoAsync(Guid formId, string thirdPartyOrderNo);

		ValueTask TagWithdrawFormPaidAsync(Guid formId);

		ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo);
	}
}
