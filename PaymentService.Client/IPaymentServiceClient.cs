using System.Text.Json;
using Stateless;

namespace Tgs.Payment.Clients;

public interface IPaymentServiceClient
{
	ValueTask<decimal> CalculateHandlingFeeAsync(Guid brandPaymentId, decimal amount, CancellationToken cancellationToken = default);

	ValueTask<int> CountUntreatedDepositFormsAsync(Guid brandId, CancellationToken cancellationToken = default);

	ValueTask<int> CountUntreatedWithdrawFormsAsync(Guid brandId, CancellationToken cancellationToken = default);

	ValueTask<Guid> CreateAgentBankInfoAsync(
		Guid brandId,
		Guid agentId,
		string agentName,
		BankCreateInfo info,
		CancellationToken cancellationToken = default);

	ValueTask<Guid> CreateDepositFormAsync(Guid createFormId, DepositFormCreateInfo createInfo, CancellationToken cancellationToken = default);

	StateMachine<PaymentFlowState, PaymentTrigger> CreateDepositStateMachine(PaymentFlowState initialState = PaymentFlowState.Init);

	StateMachine<PaymentFlowState, PaymentTrigger> CreateManualDepositStateMachine(PaymentFlowState initialState = PaymentFlowState.Init);

	ValueTask CreatePaymentAsync(PaymentCreateInfo createInfo, CancellationToken cancellationToken = default);

	ValueTask CreatePaymentChannelAsync(string channelCode, CancellationToken cancellationToken = default);

	ValueTask CreatePaymentMethodAsync(PaymentMethodCreateInfo createInfo, CancellationToken cancellationToken = default);

	ValueTask<Guid> CreatePaymentMethodChannelAsync(Guid paymentId, Guid methodId, Guid channelId, CancellationToken cancellationToken = default);

	ValueTask<Guid> CreatePlayerBankInfoAsync(Guid brandId, string playerCode, BankCreateInfo info, string? modifyUser = null, CancellationToken cancellationToken = default);

	ValueTask<Guid> CreateWithdrawFormAsync(WithdrawFormCreateInfo createInfo, CancellationToken cancellationToken = default);

	StateMachine<PaymentFlowState, PaymentTrigger> CreateWithdrawStateMachine(PaymentFlowState initialState = PaymentFlowState.Init);

	ValueTask DeleteAgentBankInfoAsync(Guid agentId, Guid bankInfoId, CancellationToken cancellationToken = default);

	ValueTask DeletePaymentMethodChannelAsync(Guid pmcId, CancellationToken cancellationToken = default);

	ValueTask DeletePlayerBankInfoAsync(string playerCode, Guid bankInfoId, CancellationToken cancellationToken = default);

	[Obsolete("Currently unsupported VipLevel and Phone Authentication on this phase")]
	IAsyncEnumerable<BrandPaymentInfo> FilterBrandPaymentsAsync(
		IEnumerable<BrandPaymentInfo> brandPaymentInfos,
		PlayerInfo playerInfo,
		IEnumerable<Guid> blockList,
		IEnumerable<Guid> whiteList,
		ILookup<Guid, Guid> brandPaymentVipLevels,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentInfo> FilterBrandPaymentsAsync(
	IEnumerable<BrandPaymentInfo> brandPaymentInfos,
	PlayerInfo playerInfo,
	IEnumerable<Guid> blockList,
	IEnumerable<Guid> whiteList,
	CancellationToken cancellationToken = default);


	IAsyncEnumerable<AgentBankInfo> GetAgentBankInfosAsync(Guid agentId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync(CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentMethodChannelInfo> GetAllPaymentMethodChannelsAsync(CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync(CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync(CancellationToken cancellationToken = default);

	ValueTask<AutoWithdrawInfo?> GetBrandAutoWithdrawSettingAsync(Guid brandId, CancellationToken cancellationToken = default);

	ValueTask<decimal> GetBrandCurrentDayNetDepositAsync(Guid brandId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(
		string playerCode,
		IEnumerable<Guid> brandPaymentIds,
		CancellationToken cancellationToken = default);

	ValueTask<BrandPaymentInfo?> GetBrandPaymentByIdAsync(Guid brandPaymentId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentPlayerInfo> GetBrandPaymentDesignatedUsersAsync(Guid brandPaymentId, CancellationToken cancellationToken = default);
	
	ValueTask<BrandPaymentPlayerAccess?> GetBrandPaymentPlayerAccessAsync(Guid brandPaymentId, CancellationToken cancellationToken= default);

	ValueTask<BrandPaymentFeeInfo?> GetBrandPaymentFeeSettingsAsync(Guid brandPaymentId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(Guid brandId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentFeeInfo> GetBrandPaymentsFeeSettingsAsync(IEnumerable<Guid> brandPaymentIds, CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipsAsync(IEnumerable<Guid> brandPaymentIds, CancellationToken cancellationToken = default);

	ValueTask<decimal> GetBrandReportDateNetDepositAsync(Guid brandId, DateOnly reportDate, CancellationToken cancellationToken = default);

	ValueTask<decimal> GetBrandSuccessRateByReportDateAsync(Guid brandId, DateOnly reportDate, CancellationToken cancellationToken = default);

	ValueTask<decimal> GetCurrentDayDepositSuccessRateAsync(Guid brandId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<TopTenDepositWithdrawInfo> GetCurrentDayDepositWithdrawTotalAmountAsync(IEnumerable<string> playerCodes, CancellationToken cancellationToken = default);

	IAsyncEnumerable<TopTenDepositWithdrawInfo> GetCurrentDayTopTenDepositPlayersAsync(Guid brandId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<TopTenDepositWithdrawInfo> GetCurrentDayTopTenWithdrawPlayersAsync(Guid brandId, CancellationToken cancellationToken = default);

	ValueTask<DepositFormInfo?> GetDepositFormAsync(Guid formId, CancellationToken cancellationToken = default);

	[Obsolete("淘汰")]
	ValueTask<DepositFormInfo?> GetDepositFormAsync(Guid brandId, Guid formId, CancellationToken cancellationToken = default);

	ValueTask<PaymentInfo?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PlayerBankInfo> GetPlayerBankInfosAsync(string playerCode, CancellationToken cancellationToken = default);

	ValueTask<PaymentOverview> GetPlayerPaymentOverviewAsync(string playerCode, CancellationToken cancellationToken = default);

	IAsyncEnumerable<string> GetPlayersByBankInfoTagAsync(Guid brandId, string tagType, string tag, CancellationToken cancellationToken = default);

	ValueTask<WithdrawFormInfo?> GetWithdrawFormAsync(Guid formId, CancellationToken cancellationToken = default);

	ValueTask<WithdrawFormInfo?> GetWithdrawFormAsync(Guid brandId, Guid formId, CancellationToken cancellationToken = default);

	ValueTask<Guid> LockDepositPaymentAsync(
		Guid formId,
		string paymentCode,
		string playerCode,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandPaymentInfo> QueryBrandPaymentsAsync(
		Guid? brandId,
		TransferType? transferType,
		bool? enabled,
		string? keyword,
		CancellationToken cancellationToken = default);

	ValueTask<DepositForms> QueryDepositFormsAsync(DepositFormQueryConditions conditions, Paging paging, CancellationToken cancellationToken = default);

	ValueTask<WithdrawForms> QueryWithdrawFormsAsync(WithdrawFormQueryConditions conditions, Paging paging, CancellationToken cancellationToken = default);

	IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(Guid brandId, string userId, UserType userType, CancellationToken cancellationToken = default);

	ValueTask ReviewDepositFormAsync(
		Guid formId,
		ReviewState state,
		string? reviewUserId,
		string? reviewUserName,
		string? remark = null,
		CancellationToken cancellationToken = default);

	ValueTask ReviewWithdrawFormAsync(
		Guid formId,
		Guid? brandPaymentId,
		ReviewState state,
		string? reviewUserId,
		string? reviewUserName,
		string? remark = null,
		string? playerRemark = null,
		CancellationToken cancellationToken = default);

	ValueTask SetDefaultPlayerBankInfoAsync(
		string playerCode,
		Guid bankInfoId,
		string modifyUser,
		CancellationToken cancellationToken = default);

	ValueTask SubmitDepositFormStatusAsync(
		Guid formId,
		PaymentFlowState? state = null,
		string? paymentReturn = null,
		decimal? actualAmount = null,
		string? thirdPartyOrderNo = null,
		int? promotionPlayerId = null,
		decimal? handlingFee = null,
		CancellationToken cancellationToken = default);

	ValueTask SubmitWithdrawFormStatusAsync(
		Guid formId,
		PaymentFlowState? state = null,
		string? paymentReturn = null,
		decimal? actualAmount = null,
		string? thirdPartyOrderNo = null,
		string? paidUserId = null,
		string? paidUserName = null,
		CancellationToken cancellationToken = default);

	ValueTask UnlockDepositPaymentAsync(
		Guid formId,
		string paymentCode,
		string playerCode,
		CancellationToken cancellationToken = default);

	ValueTask UpdateBrandAutoWithdrawAsync(
		Guid brandId,
		bool enableAutoWithdraw,
		decimal amountUpperLimit,
		IEnumerable<Guid> brandPaymentIds,
		string modifyUserId,
		CancellationToken cancellationToken = default);

	ValueTask UpdateBrandPaymentExtendedDataAsync(Guid brandPaymentId, JsonDocument extendedData, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentStatistics> QueryPaymentStatisticsAsync(
		DateOnly start,
		DateOnly end,
		IEnumerable<string>? userIds = null,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<PaymentStatistics> Query5minPaymentStatisticsAsync(
		DateTime start,
		DateTime end,
		CancellationToken cancellationToken = default);
}
