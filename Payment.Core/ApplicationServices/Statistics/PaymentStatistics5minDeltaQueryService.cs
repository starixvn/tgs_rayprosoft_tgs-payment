using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Statistics;

internal class PaymentStatistics5minDeltaQueryService(
	IStatisticsRepository statisticsRepository,
	TimeProvider timeProvider)
	: IQueryService<PaymentStatistics5minDeltaQuery, UserPaymentStatisticsDelta>
{
	public IAsyncEnumerable<UserPaymentStatisticsDelta> QueryAsync(
		PaymentStatistics5minDeltaQuery query,
		CancellationToken cancellationToken = default)
	{
		var stopId = GetCurrentStopId();

		return statisticsRepository.Query5minPaymentStatisticsDeltaAsync(
			query.AnchorId,
			stopId,
			cancellationToken);
	}

	private Guid GetCurrentStopId()
	{
		var currentTime = timeProvider.GetUtcNow().ToOffset(PaymentUtility.ReportOffset);
		var stopTimestamp = currentTime.FloorFiveMinutesTime().ToUnixTimeMilliseconds();
		Span<byte> stopId = stackalloc byte[16];

		var timestampBuffer = MemoryMarshal.AsBytes([stopTimestamp]);

		ref var a = ref MemoryMarshal.AsRef<int>(stopId[0..4]);
		a = MemoryMarshal.AsRef<int>(timestampBuffer[2..6]);

		ref var b = ref MemoryMarshal.AsRef<short>(stopId[4..6]);
		b = MemoryMarshal.AsRef<short>(timestampBuffer[0..2]);

		return new Guid(stopId);
	}
}
