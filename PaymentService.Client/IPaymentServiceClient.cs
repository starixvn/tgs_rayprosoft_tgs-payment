using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tgs.Payment.Clients
{
	public interface IPaymentServiceClient
	{
		ValueTask<Guid> CreateAgentBankInfoAsync(Guid brandId, Guid agentId, string agentName, BankCreateInfo info);

		ValueTask<Guid> CreateDepositFormAsync(Guid createFormId, DepositFormCreateInfo createInfo);

		ValueTask<Guid> CreatePlayerBankInfoAsync(Guid brandId, string playerCode, BankCreateInfo info);

		ValueTask<Guid> CreateWithdrawFormAsync(WithdrawFormCreateInfo createInfo);

		ValueTask DeleteAgentBankInfoAsync(Guid agentId, Guid bankInfoId);

		ValueTask DeletePlayerBankInfoAsync(string playerCode, Guid bankInfoId);

		IAsyncEnumerable<AgentBankInfo> GetAgentBankInfosAsync(Guid agentId);

		IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync();

		IAsyncEnumerable<PaymentMethodChannelInfo> GetAllPaymentMethodChannelsAsync();

		IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync();

		IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync();

		IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(
			string playerCode,
			IEnumerable<Guid> brandPaymentIds);

		ValueTask<BrandPaymentInfo?> GetBrandPaymentByIdAsync(Guid brandPaymentId);

		IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(Guid brandId);

		IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipsAsync(IEnumerable<Guid> brandPaymentIds);

		ValueTask<DepositFormInfo?> GetDepositFormAsync(Guid brandId, Guid formId);

		ValueTask<PaymentInfo?> GetPaymentByIdAsync(Guid paymentId);

		IAsyncEnumerable<PlayerBankInfo> GetPlayerBankInfosAsync(string playerCode);

		ValueTask<WithdrawFormInfo?> GetWithdrawFormAsync(Guid brandId, Guid formId);

		ValueTask<PagingResponse<DepositFormInfo>> QueryDepositFormsAsync(DepositFormQueryConditions conditions, Paging paging);

		ValueTask<PagingResponse<WithdrawFormInfo>> QueryWithdrawFormsAsync(WithdrawFormQueryConditions conditions, Paging paging);

		ValueTask ReviewDepositFormAsync(Guid formId, ReviewState state, string? remark = null);

		ValueTask ReviewWithdrawFormAsync(Guid formId, Guid? brandPaymentId, ReviewState state, string? remark = null);

		ValueTask SubmitDepositFormStatusAsync(Guid formId, PaymentFlowState state, string? paymentReturn = null);

		ValueTask SubmitWithdrawFormStatusAsync(
			Guid formId,
			PaymentFlowState state,
			string? paymentReturn = null,
			decimal? actualAmount = null);
	}
}
