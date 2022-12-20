using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Paging;

namespace Tgs.Payment;

public partial interface IPaymentRepository
{
	ValueTask<decimal> CalculateWithdrawTotalAmountAsync(WithdrawFormQueryConditions conditions, CancellationToken cancellationToken = default);

	ValueTask<int> CountWithdrawFormsAsync(WithdrawFormQueryConditions conditions, CancellationToken cancellationToken = default);

	ValueTask CreateWithdrawFormAsync(Guid createFormId, WithdrawFormCreateInfo form, CancellationToken cancellationToken = default);

	ValueTask<WithdrawFormInfo?> GetWithdrawFormByIdAsync(Guid formId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
		WithdrawFormQueryConditions conditions,
		Paging<WithdrawFormInfo> paging,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
		WithdrawFormQueryConditions conditions,
		CancellationToken cancellationToken = default);

	ValueTask SignOffWithdrawFormAsync(
		Guid formId,
		Guid? brandPaymentId,
		string? applyInfo,
		PaymentFlowState reviewStatus,
		string? reviewedUserId,
		string? reviewedUserName,
		string? remark = null,
		decimal? handlingFee = null,
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

	ValueTask CreateWithdrawFormEventAsync(
		Guid eventId,
		Guid formId,
		DepositFormAction action,
		JsonDocument changeSets,
		string? modifier,
		CancellationToken cancellationToken = default);

	ValueTask UpdateWithdrawFormSnapshotAsync(WithdrawFormSnapshot snapshot, Guid snapshotId, DateTime updatedOn, CancellationToken cancellationToken = default);
}
