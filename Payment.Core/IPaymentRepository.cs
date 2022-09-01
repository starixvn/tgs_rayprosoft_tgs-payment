using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Paging;

namespace Tgs.Payment;

public interface IPaymentRepository
{
	ValueTask<decimal> CalculateDepositTotalAmountAsync(DepositFormQueryConditions conditions, CancellationToken cancellationToken = default);

	ValueTask<decimal> CalculateWithdrawTotalAmountAsync(WithdrawFormQueryConditions conditions, CancellationToken cancellationToken = default);

	ValueTask<int> CountDepositFormsAsync(DepositFormQueryConditions conditions, CancellationToken cancellationToken = default);

	ValueTask<int> CountWithdrawFormsAsync(WithdrawFormQueryConditions conditions, CancellationToken cancellationToken = default);

	ValueTask CreateAgentBankInfoAsync(AgentBankInfo agentBankInfo, string modifyUser, CancellationToken cancellationToken = default);

	ValueTask CreateBankInfoAsync(PlayerBankInfo bankInfo, string modifyUser, CancellationToken cancellationToken = default);

	ValueTask CreateDepositFormAsync(Guid createFormId, DepositFormCreateInfo form, CancellationToken cancellationToken = default);

	ValueTask CreatePaymentAsync(PaymentCreateInfo createInfo, CancellationToken cancellationToken = default);

	ValueTask CreatePaymentChannelAsync(string channelCode, CancellationToken cancellationToken = default);

	ValueTask CreatePaymentMethodAsync(string methodCode, string bankType, CancellationToken cancellationToken = default);

	ValueTask<Guid> CreatePaymentMethodChannelAsync(Guid paymentId, Guid methodId, Guid channelId, CancellationToken cancellationToken = default);

	ValueTask CreateWithdrawFormAsync(Guid createFormId, WithdrawFormCreateInfo form, CancellationToken cancellationToken = default);

	ValueTask DeleteAgentBankInfoAsync(Guid agentId, Guid bankInfoId, CancellationToken cancellationToken = default);

	ValueTask DeleteBankInfoAsync(string playerCode, Guid bankInfoId, CancellationToken cancellationToken = default);

	ValueTask DeletePaymentMethodChannelAsync(Guid pmcId, CancellationToken cancellationToken = default);

	ValueTask<AgentBankInfo> GetAgentBankInfoByIdAsync(Guid id, CancellationToken cancellationToken = default);

	IAsyncEnumerable<AgentBankInfo> GetAgentBankInfosByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync(CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentMethodChannelInfo> GetAllPaymentMethodChannelsAsync(CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync(CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync(CancellationToken cancellationToken = default);

	ValueTask<PlayerBankInfo?> GetBankInfoByIdAsync(Guid id, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PlayerBankInfo> GetBankInfosByPlayerCodeAsync(string playerCode, CancellationToken cancellationToken = default);

	ValueTask<BrandInfo> GetBrandInfoByIdAsync(Guid id, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(
		IEnumerable<Guid> brandPaymentIds,
		string playerCode,
		CancellationToken cancellationToken = default);

	ValueTask<BrandPaymentInfo> GetBrandPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentPlayerInfo> GetBrandPaymentDesignatedUsersAsync(Guid id, CancellationToken cancellationToken = default);

	ValueTask<BrandPaymentFeeInfo> GetBrandPaymentFeeSettingsAsync(Guid id, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(Guid brandId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipListAsync(IEnumerable<Guid> brandPaymentIds, CancellationToken cancellationToken = default);

	ValueTask<DepositFormInfo?> GetDepositFormByIdAsync(Guid id, CancellationToken cancellationToken = default);

	ValueTask<PaymentInfo> GetPaymentAsync(Guid id, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentMethodChannelInfo> GetPaymentMethodChannelsAsync(CancellationToken cancellationToken = default);

	ValueTask<WithdrawFormInfo?> GetWithdrawFormByIdAsync(Guid formId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentInfo> QueryBrandPaymentsAsync(Guid? brandId, TransferType? transferType, bool? enabled, string? keyword, CancellationToken cancellationToken = default);

	IAsyncEnumerable<DepositFormInfo> QueryDepositFormsAsync(
		DepositFormQueryConditions conditions,
		Paging<DepositFormInfo> paging,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
		WithdrawFormQueryConditions conditions,
		Paging<WithdrawFormInfo> paging,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
		WithdrawFormQueryConditions conditions,
		CancellationToken cancellationToken = default);

	ValueTask RemoveBankInfoTagsAsync(Guid brandId, Guid bankInfoId, CancellationToken cancellationToken = default);

	ValueTask SetBankInfoTagAsync(
		Guid brandId,
		Guid bankInfoId,
		string tagType,
		string playerCode,
		string tag,
		CancellationToken cancellationToken = default);

	ValueTask SetDefaultPlayerBankInfoAsync(
		string playerCode,
		Guid bankInfoId,
		string modifyUser,
		CancellationToken cancellationToken = default);

	ValueTask SignOffDepositFormAsync(
		Guid formId,
		ReviewState reviewStatus,
		string? reviewedUserId,
		string? reviewedUserName,
		string? remark = null,
		CancellationToken cancellationToken = default);

	ValueTask SignOffWithdrawFormAsync(
		Guid formId,
		Guid? brandPaymentId,
		string? applyInfo,
		PaymentFlowState reviewStatus,
		string? reviewedUserId,
		string? reviewedUserName,
		string? remark = null,
		CancellationToken cancellationToken = default);

	ValueTask SubmitDepositFormStatusAsync(
		Guid formId,
		PaymentFlowState? status,
		string? paymentReturn,
		decimal? actualAmount,
		string? thirdPartyOrderNo,
		int? promotionPlayerId,
		CancellationToken cancellationToken = default);

	ValueTask SubmitWithdrawFormStatusAsync(
		Guid formId,
		PaymentFlowState? status,
		string? paymentReturn,
		decimal? actualAmount,
		string? thirdPartyOrderNo,
		string? paidUserId,
		string? paidUserName,
		CancellationToken cancellationToken = default);
}
