using Microsoft.Extensions.DependencyInjection;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Statistics;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Payment.IntegrationTests;

public class QueryStatisticsTests
{
	[Test]
	public async Task 整合測試_查詢每日統計資料()
	{
		var fakeQueryService = Substitute.For<IQueryService<PaymentStatisticsQuery, UserPaymentSummaryStatistics>>();
		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeQueryService));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var start = DateOnly.Parse("2024-1-29");
		var end = DateOnly.Parse("2024-1-31");
		var player = "player1";

		_ = fakeQueryService.QueryAsync(Arg.Any<PaymentStatisticsQuery>(), Arg.Any<CancellationToken>())
			.Returns(new[]
			{
				new UserPaymentSummaryStatistics(
					DateTime.ParseExact("2024-01-29T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
					player,
					Tgs.Payment.UserType.Player,
					100M,
					0M,
					1,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null)
			}.ToAsyncEnumerable());

		var actual = await sut.QueryPaymentStatisticsAsync(start, end, [player]).ToArrayAsync();

		Assert.That(actual, Is.Not.Null);
		Assert.That(actual.Length, Is.EqualTo(1));
	}

	[Test]
	public async Task 整合測試_查詢5分鐘統計資料()
	{
		var fakeQueryService = Substitute.For<IQueryService<PaymentStatistics5minQuery, UserPaymentSummaryStatistics>>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeQueryService));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var start = DateTime.ParseExact("2024-01-30T10:50:00.0000000+08:00", "o", null);
		var end = DateTime.ParseExact("2024-01-30T10:55:00.0000000+08:00", "o", null);

		_ = fakeQueryService.QueryAsync(Arg.Any<PaymentStatistics5minQuery>(), Arg.Any<CancellationToken>())
			.Returns(new[]
			{
				new UserPaymentSummaryStatistics(
					DateTime.ParseExact("2024-01-30T10:50:00.0000000+08:00", "o", null),
					"test1",
					Tgs.Payment.UserType.Player,
					100M,
					0M,
					1,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null)
			}.ToAsyncEnumerable());

		var actual = await sut.Query5minPaymentStatisticsAsync(start, end).ToArrayAsync();

		Assert.That(actual, Is.Not.Null);
		Assert.That(actual.Length, Is.EqualTo(1));
	}
}
