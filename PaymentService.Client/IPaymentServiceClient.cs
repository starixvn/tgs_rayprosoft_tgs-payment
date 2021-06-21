using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tgs.Payment.Clients
{
	public interface IPaymentServiceClient
	{
		ValueTask<Guid> CreateDepositFormAsync(DepositFormCreateInfo createInfo);

		ValueTask<Guid> CreatePlayerBankInfoAsync(Guid brandId, string playerCode, BankCreateInfo info);

		IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync();

		IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethosAsync();

		IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync();

		ValueTask<DepositFormInfo?> GetDepositFormAsync(Guid brandId, Guid formId);

		ValueTask<PaymentInfo?> GetPaymentByIdAsync(Guid paymentId);

		IAsyncEnumerable<PlayerBankInfo> GetPlayerBankInfosAsync(string playerCode);

		ValueTask<PagingResponse<DepositFormInfo>> QueryDepositFormsAsync(DepositFormQueryConditions conditions, Paging paging);

		ValueTask ReviewDepositFormAsync(Guid formId, ReviewState state, string? remark = null);

		ValueTask SubmitDepositFormStatusAsync(Guid formId, PaymentFlowState state, string? paymentReturn = null);
	}
}
