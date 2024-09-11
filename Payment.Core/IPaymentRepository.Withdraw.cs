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

	ValueTask CreateWithdrawFormEventAsync(
		Guid eventId,
		Guid formId,
		EventAction action,
		JsonDocument changeSets,
		string? modifier,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<TopTenDepositWithdrawInfo> GetCurrentDayTopTenWithdrawPlayersAsync(Guid brandId, CancellationToken cancellationToken = default);

	ValueTask<WithdrawFormSnapshot?> GetCurrentWithdrawFormSnapshotAsync(Guid id, CancellationToken cancellationToken = default);

	ValueTask<WithdrawFormInfo?> GetWithdrawFormByIdAsync(Guid formId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
		WithdrawFormQueryConditions conditions,
		Paging<WithdrawFormInfo> paging,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
		WithdrawFormQueryConditions conditions,
		bool limitQuery = true,
		CancellationToken cancellationToken = default);

	ValueTask UpdateWithdrawFormSnapshotAsync(WithdrawFormSnapshot snapshot, Guid snapshotId, DateTime updatedOn, CancellationToken cancellationToken = default);

	ValueTask SaveApplyInfoAsync(Guid formId, string? appyInfo, CancellationToken cancellationToken = default);
}
