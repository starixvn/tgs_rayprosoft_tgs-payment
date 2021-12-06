using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TGS.Paging;

namespace Tgs.Payment
{
	public interface IPaymentRepository
	{
		ValueTask<decimal> CalculateDepositTotalAmountAsync(DepositFormQueryConditions conditions);

		ValueTask<decimal> CalculateWithdrawTotalAmountAsync(WithdrawFormQueryConditions conditions);

		ValueTask<int> CountDepositFormsAsync(DepositFormQueryConditions conditions);

		ValueTask<int> CountWithdrawFormsAsync(WithdrawFormQueryConditions conditions);

		ValueTask CreateAgentBankInfoAsync(AgentBankInfo agentBankInfo, string modifyUser);

		ValueTask CreateBankInfoAsync(PlayerBankInfo bankInfo, string modifyUser);

		ValueTask CreateDepositFormAsync(Guid createFormId, DepositFormCreateInfo form);

		ValueTask CreatePaymentAsync(PaymentCreateInfo createInfo);

		ValueTask CreatePaymentChannelAsync(string channelCode);

		ValueTask CreatePaymentMethodAsync(string methodCode, string bankType);

		ValueTask CreateWithdrawFormAsync(Guid createFormId, WithdrawFormCreateInfo form);

		ValueTask DeleteAgentBankInfoAsync(Guid agentId, Guid bankInfoId);

		ValueTask DeleteBankInfoAsync(string playerCode, Guid bankInfoId);

		ValueTask<AgentBankInfo> GetAgentBankInfoByIdAsync(Guid id);

		IAsyncEnumerable<AgentBankInfo> GetAgentBankInfosByAgentIdAsync(Guid agentId);

		IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync();

		IAsyncEnumerable<PaymentMethodChannelInfo> GetAllPaymentMethodChannelsAsync();

		IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync();

		IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync();

		ValueTask<PlayerBankInfo> GetBankInfoByIdAsync(Guid id);

		IAsyncEnumerable<PlayerBankInfo> GetBankInfosByPlayerCodeAsync(string playerCode);

		ValueTask<BrandInfo> GetBrandInfoByIdAsync(Guid id);

		IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(IEnumerable<Guid> brandPaymentIds, string playerCode);

		ValueTask<BrandPaymentInfo> GetBrandPaymentByIdAsync(Guid id);

		IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(Guid brandId);

		IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipListAsync(IEnumerable<Guid> brandPaymentIds);

		ValueTask<DepositFormInfo?> GetDepositFormByIdAsync(Guid id);

		ValueTask<PaymentInfo> GetPaymentAsync(Guid id);

		IAsyncEnumerable<PaymentMethodChannelInfo> GetPaymentMethodChannelsAsync();

		ValueTask<WithdrawFormInfo?> GetWithdrawFormByIdAsync(Guid formId);

		IAsyncEnumerable<DepositFormInfo> QueryDepositFormsAsync(
			DepositFormQueryConditions conditions,
			Paging<DepositFormInfo> paging);

		IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
			WithdrawFormQueryConditions conditions,
			Paging<WithdrawFormInfo> paging);

		IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
			WithdrawFormQueryConditions conditions);

		ValueTask SignOffDepositFormAsync(
			Guid formId,
			ReviewState reviewStatus,
			string? reviewedUserId,
			string? reviewedUserName,
			string? remark = null);

		ValueTask SignOffWithdrawFormAsync(
			Guid formId,
			Guid? brandPaymentId,
			string? applyInfo,
			ReviewState reviewStatus,
			string? reviewedUserId,
			string? reviewedUserName,
			string? remark = null);

		ValueTask SubmitDepositFormStatusAsync(
			Guid formId,
			PaymentFlowState? status,
			string? paymentReturn,
			decimal? actualAmount,
			string? thirdPartyOrderNo,
			int? promotionPlayerId);

		ValueTask SubmitWithdrawFormStatusAsync(
			Guid formId,
			PaymentFlowState? status,
			string? paymentReturn,
			decimal? actualAmount,
			string? thirdPartyOrderNo,
			string? paidUserId,
			string? paidUserName);
	}
}
