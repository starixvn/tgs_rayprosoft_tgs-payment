using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tgs.Payment.Clients
{
	public interface IPaymentServiceClient
	{
		Task<Guid> CreateDepositFormAsync(DepositFormCreateInfo createInfo);

		IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync();

		IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethosAsync();

		IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync();

		ValueTask<DepositFormInfo?> GetDepositFormAsync(Guid brandId, Guid formId);

		ValueTask<PaymentInfo?> GetPaymentByIdAsync(Guid paymentId);

		ValueTask<PagingResponse<DepositFormInfo>> QueryDepositFormsAsync(DepositFormQueryConditions conditions, Paging paging);

		ValueTask ReviewDepositFormAsync(Guid formId, ReviewState state, string? remark = null);

		ValueTask SubmitDepositFormStatusAsync(Guid formId, PaymentFlowState state, string? paymentReturn = null);
	}
}
