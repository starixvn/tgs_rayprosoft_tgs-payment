using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TGS.Paging;

namespace Tgs.Payment
{
	public interface IPaymentRepository
	{
		ValueTask<int> CountDepositFormsAsync(DepositFormQueryConditions conditions);

		ValueTask CreateAgentBankInfoAsync(AgentBankInfo agentBankInfo, string modifyUser);

		ValueTask CreateBankInfoAsync(BankInfo bankInfo, string modifyUser);

		ValueTask CreateDepositFormAsync(Guid createFormId, DepositFormCreateInfo form);

		ValueTask CreateWithdrawFormAsync(Guid createFormId, WithdrawFormCreateInfo form);

		ValueTask DeleteAgentBankInfoAsync(Guid agentId, Guid bankInfoId);

		ValueTask DeleteBankInfoAsync(string playerCode, Guid bankInfoId);

		ValueTask<AgentBankInfo> GetAgentBankInfoByIdAsync(Guid id);

		IAsyncEnumerable<AgentBankInfo> GetAgentBankInfosByAgentIdAsync(Guid agentId);

		IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync();

		IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync();

		IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync();

		ValueTask<BankInfo> GetBankInfoByIdAsync(Guid id);

		IAsyncEnumerable<BankInfo> GetBankInfosByPlayerCodeAsync(string playerCode);

		IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(IEnumerable<Guid> brandPaymentIds, string playerCode);

		ValueTask<BrandPaymentInfo> GetBrandPaymentByIdAsync(Guid id);

		IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(Guid brandId);

		IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipListAsync(IEnumerable<Guid> brandPaymentIds);

		ValueTask<DepositFormInfo> GetDepositFormByIdAsync(Guid id);

		ValueTask<PaymentInfo> GetPaymentAsync(Guid id);

		IAsyncEnumerable<PaymentMethodChannelInfo> GetPaymentMethodChannelsAsync();

		ValueTask<WithdrawFormInfo> GetWithdrawFormByIdAsync(Guid formId);

		IAsyncEnumerable<DepositFormInfo> QueryDepositFormsAsync(
			DepositFormQueryConditions conditions,
			Paging<DepositFormInfo> paging);

		IAsyncEnumerable<DepositFormInfo> QueryDepositFormsAsync(
			DepositFormQueryConditions conditions);

		ValueTask SignOffDepositFormAsync(Guid formId, ReviewState reviewStatus, string? remark = null);

		ValueTask SignOffWithdrawFormAsync(
			Guid formId,
			Guid? brandPaymentId,
			string? applyInfo,
			ReviewState reviewStatus,
			string? remark = null);

		ValueTask SubmitDepositFormStatusAsync(Guid formId, PaymentFlowState status, string? paymentReturn);
	}
}
