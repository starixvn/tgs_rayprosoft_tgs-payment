using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Statistics;

namespace Payment.UnitTests;

public class PaymentStatistics5minDeltaQueryServiceTests
{
	[Test]
	public async Task PaymentStatistics5minDeltaQueryService_查詢5分鐘差異資料串流()
	{
		var fakeStatisticsRepository = Substitute.For<IStatisticsRepository>();
		var fakeTimeProvider = Substitute.For<TimeProvider>();

		var sut = new PaymentStatistics5minDeltaQueryService(
			fakeStatisticsRepository,
			fakeTimeProvider);

		var anchorId = Guid.Parse("01853878-42af-0000-0001-07e634d6c88a");

		var query = new PaymentStatistics5minDeltaQuery(anchorId);

		var currentTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64("0186387842af", 16));

		var stopId = Guid.Parse("01863875-F520-0000-0000-000000000000");

		_ = fakeTimeProvider.GetUtcNow().Returns(currentTime.ToUniversalTime());

		_ = await sut.QueryAsync(query).ToArrayAsync();

		_ = fakeStatisticsRepository.Received(1)
			.Query5minPaymentStatisticsDeltaAsync(Arg.Is(anchorId), Arg.Is(stopId));
	}
}
