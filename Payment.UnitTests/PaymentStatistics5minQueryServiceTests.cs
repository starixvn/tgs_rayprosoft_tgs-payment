using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Statistics;

namespace Payment.UnitTests;

public class PaymentStatistics5minQueryServiceTests
{
	[Test]
	public async Task PaymentStatistics5minQueryService_查詢5分鐘統計資料()
	{
		var fakeStatisticsRepository = Substitute.For<IStatisticsRepository>();

		var sut = new PaymentStatistics5minQueryService(fakeStatisticsRepository);
		var query = new PaymentStatistics5minQuery(
			DateTime.ParseExact("2024-01-30T10:45:00.0000000+08:00", "o", null),
			DateTime.ParseExact("2024-01-30T10:50:00.0000000+08:00", "o", null));

		_ = await sut.QueryAsync(query).ToArrayAsync();

		_ = fakeStatisticsRepository.Received(1)
			.Query5minPaymentStatisticsAsync(
				Arg.Is(query.Start),
				Arg.Is(query.End),
				Arg.Is((string[])null));
	}
}
