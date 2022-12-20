using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Paging;

namespace Tgs.Payment;

public partial interface IPaymentRepository
{
	ValueTask<decimal> CalculateDepositTotalAmountAsync(DepositFormQueryConditions conditions, CancellationToken cancellationToken = default);

	ValueTask<int> CountDepositFormsAsync(DepositFormQueryConditions conditions, CancellationToken cancellationToken = default);

	ValueTask CreateDepositFormEventAsync(
		Guid eventId,
		Guid formId,
		DepositFormAction action,
		JsonDocument changeSets,
		string? modifier,
		CancellationToken cancellationToken = default);

	ValueTask<DepositFormSnapshot?> GetCurrentDepositFormSnapshotAsync(Guid id, CancellationToken cancellationToken = default);

	ValueTask<DepositFormInfo?> GetDepositFormByIdAsync(Guid id, CancellationToken cancellationToken = default);

	IAsyncEnumerable<DepositFormInfo> QueryDepositFormsAsync(
		DepositFormQueryConditions conditions,
		Paging<DepositFormInfo> paging,
		CancellationToken cancellationToken = default);

	ValueTask UpdateDepositFormSnapshotAsync(DepositFormSnapshot snapshot, Guid snapshotId, DateTime updatedOn, CancellationToken cancellationToken = default);
}
