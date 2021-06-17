using System;
using System.Threading.Tasks;

namespace Tgs.Payment.Clients
{
	public interface IPaymentServiceClient
	{
		Task<Guid> CreateDepositFormAsync(DepositFormCreateInfo createInfo);

		ValueTask<DepositFormInfo?> GetDepositFormAsync(Guid brandId, Guid formId);

		ValueTask<PagingResponse<DepositFormInfo>> QueryDepositFormsAsync(DepositFormQueryConditions conditions, Paging paging);

		ValueTask ReviewDepositFormAsync(Guid formId, ReviewState state, string? remark = null);

		ValueTask SubmitDepositFormStatusAsync(Guid formId, PaymentFlowState state, string? paymentReturn = null);
	}
}
