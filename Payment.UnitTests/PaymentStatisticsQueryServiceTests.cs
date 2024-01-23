using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Statistics;

namespace Payment.UnitTests;

public class PaymentStatisticsQueryServiceTests
{
	[Test]
	public async Task PaymentStatisticsQueryService_查詢每日存款統計()
	{
		var fakeStatisticsRepository = Substitute.For<IStatisticsRepository>();

		var sut = new PaymentStatisticsQueryService(TimeProvider.System, fakeStatisticsRepository);

		var query = new PaymentStatisticsQuery(
			DateOnly.Parse("2023-12-1"),
			DateOnly.Parse("2023-12-2"),
			null);

		_ = await sut.QueryAsync(query).ToArrayAsync();

		_ = fakeStatisticsRepository.Received(1)
			.QueryDailyPaymentStatisticsAsync(
				Arg.Is(query.Start),
				Arg.Is(query.End),
				Arg.Is(query.UserIds));
		_ = fakeStatisticsRepository.DidNotReceive()
			.Query5minPaymentStatisticsAsync(
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<string[]>());
		_ = fakeStatisticsRepository.DidNotReceive()
			.QueryHourPaymentStatisticsAsync(
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<string[]>());
	}

	[Test]
	public async Task PaymentStatisticsQueryService_查詢當日存款統計_會使用小時及5分鐘資料()
	{
		var fakeStatisticsRepository = Substitute.For<IStatisticsRepository>();
		var fakeTimeProvider = Substitute.For<TimeProvider>();

		var sut = new PaymentStatisticsQueryService(fakeTimeProvider, fakeStatisticsRepository);

		var query = new PaymentStatisticsQuery(
			DateOnly.Parse("2023-12-1"),
			DateOnly.Parse("2023-12-2"),
			null);

		_ = fakeTimeProvider.GetUtcNow()
			.Returns(DateTimeOffset.ParseExact("2023-12-01T09:56:00.0000000Z", "o", null));

		_ = await sut.QueryAsync(query).ToArrayAsync();

		_ = fakeStatisticsRepository.Received(1)
			.QueryDailyPaymentStatisticsAsync(
				Arg.Is(query.Start),
				Arg.Is(query.End),
				Arg.Is(query.UserIds));
		_ = fakeStatisticsRepository.Received(1)
			.Query5minPaymentStatisticsAsync(
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<string[]>());
		_ = fakeStatisticsRepository.Received(1)
			.QueryHourPaymentStatisticsAsync(
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<string[]>());
	}

	[Test]
	public async Task PaymentStatisticsQueryService_查詢每日統計_資料比對()
	{
		var fakeStatisticsRepository = Substitute.For<IStatisticsRepository>();

		var sut = new PaymentStatisticsQueryService(TimeProvider.System, fakeStatisticsRepository);

		var query = new PaymentStatisticsQuery(
			DateOnly.Parse("2023-12-1"),
			DateOnly.Parse("2023-12-2"),
			null);

		var dailyData = new[]
		{
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-01T00:00:00.0000000+08:00", "o", null),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.UtcNow,
				null,
				null,
				null,
				null,
				null,
				null)
		};

		_ = fakeStatisticsRepository.QueryDailyPaymentStatisticsAsync(
			Arg.Is(query.Start),
			Arg.Is(query.End),
			Arg.Is(query.UserIds))
			.Returns(dailyData.ToAsyncEnumerable());

		var actual = await sut.QueryAsync(query).ToArrayAsync();

		CollectionAssert.AreEqual(dailyData, actual);
	}

	[Test]
	public async Task PaymentStatisticsQueryService_查詢每日統計_未完成每日匯總的資料會查詢小時及5分的資料來輸出()
	{
		var fakeStatisticsRepository = Substitute.For<IStatisticsRepository>();
		var fakeTimeProvider = Substitute.For<TimeProvider>();

		var sut = new PaymentStatisticsQueryService(fakeTimeProvider, fakeStatisticsRepository);

		var query = new PaymentStatisticsQuery(
			DateOnly.Parse("2023-12-1"),
			DateOnly.Parse("2023-12-4"),
			null);

		var currentTime = DateTimeOffset.ParseExact("2023-12-03T07:30:00.0000000+08:00", "o", null);
		_ = fakeTimeProvider.GetUtcNow()
			.Returns(currentTime);

		var dailyData = new[]
		{
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-01T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.UtcNow,
				null,
				null,
				null,
				null,
				null,
				null)
		};

		var hourData = new[]
		{
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-02T01:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.ParseExact("2023-12-02T01:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-02T03:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.ParseExact("2023-12-02T03:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-02T23:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.ParseExact("2023-12-02T23:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-03T03:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.ParseExact("2023-12-03T03:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null)
		};

		var fiveMinutesData = new[] {
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-03T07:10:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.ParseExact("2023-12-03T07:10:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-03T07:20:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.ParseExact("2023-12-03T07:20:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
		};

		_ = fakeStatisticsRepository.QueryDailyPaymentStatisticsAsync(
			Arg.Is(query.Start),
			Arg.Is(query.End),
			Arg.Is(query.UserIds))
			.Returns(dailyData.ToAsyncEnumerable());

		_ = fakeStatisticsRepository.QueryHourPaymentStatisticsAsync(
			Arg.Is(DateTime.ParseExact("2023-12-02T00:00:00.0000000+08:00", "o", null).ToUniversalTime()),
			Arg.Is(currentTime.UtcDateTime),
			Arg.Any<string[]>())
			.Returns(hourData.ToAsyncEnumerable());

		_ = fakeStatisticsRepository.Query5minPaymentStatisticsAsync(
			Arg.Is(DateTime.ParseExact("2023-12-03T07:00:00.0000000+08:00", "o", null).ToUniversalTime()),
			Arg.Is(currentTime.UtcDateTime),
			Arg.Any<string[]>())
			.Returns(fiveMinutesData.ToAsyncEnumerable());

		var expected = new[]
		{
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-01T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				dailyData[0].FirstDepositOn,
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-02T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				300M,
				0M,
				3,
				100M,
				0M,
				DateTime.ParseExact("2023-12-02T01:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-03T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				300M,
				0M,
				3,
				100M,
				0M,
				DateTime.ParseExact("2023-12-03T03:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null)
		};

		var actual = await sut.QueryAsync(query).ToArrayAsync();

		CollectionAssert.AreEqual(expected, actual);
	}

	[Test]
	public async Task PaymentStatisticsQueryService_查詢每日統計_如果已經匯總完成的話_就會使用已完成的資料()
	{
		var fakeStatisticsRepository = Substitute.For<IStatisticsRepository>();
		var fakeTimeProvider = Substitute.For<TimeProvider>();

		var sut = new PaymentStatisticsQueryService(fakeTimeProvider, fakeStatisticsRepository);

		var query = new PaymentStatisticsQuery(
			DateOnly.Parse("2023-12-1"),
			DateOnly.Parse("2023-12-4"),
			null);

		var currentTime = DateTimeOffset.ParseExact("2023-12-03T07:30:00.0000000+08:00", "o", null);
		_ = fakeTimeProvider.GetUtcNow()
			.Returns(currentTime);

		var dailyData = new[]
		{
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-01T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.UtcNow,
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-02T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				300M,
				0M,
				3,
				100M,
				0M,
				DateTime.ParseExact("2023-12-02T01:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
		};

		var hourData = new[]
		{
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-03T03:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.ParseExact("2023-12-03T03:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null)
		};

		var fiveMinutesData = new[] {
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-03T07:10:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.ParseExact("2023-12-03T07:10:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-03T07:20:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				DateTime.ParseExact("2023-12-03T07:20:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
		};

		_ = fakeStatisticsRepository.QueryDailyPaymentStatisticsAsync(
			Arg.Is(query.Start),
			Arg.Is(query.End),
			Arg.Is(query.UserIds))
			.Returns(dailyData.ToAsyncEnumerable());

		_ = fakeStatisticsRepository.QueryHourPaymentStatisticsAsync(
			Arg.Is(DateTime.ParseExact("2023-12-03T00:00:00.0000000+08:00", "o", null).ToUniversalTime()),
			Arg.Is(currentTime.UtcDateTime),
			Arg.Any<string[]>())
			.Returns(hourData.ToAsyncEnumerable());

		_ = fakeStatisticsRepository.Query5minPaymentStatisticsAsync(
			Arg.Is(DateTime.ParseExact("2023-12-03T07:00:00.0000000+08:00", "o", null).ToUniversalTime()),
			Arg.Is(currentTime.UtcDateTime),
			Arg.Any<string[]>())
			.Returns(fiveMinutesData.ToAsyncEnumerable());

		var expected = new[]
		{
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-01T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				100M,
				0M,
				1,
				100M,
				0M,
				dailyData[0].FirstDepositOn,
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-02T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				300M,
				0M,
				3,
				100M,
				0M,
				DateTime.ParseExact("2023-12-02T01:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null),
			new UserPaymentSummaryStatistics(
				DateTime.ParseExact("2023-12-03T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				"test1",
				UserType.Player,
				300M,
				0M,
				3,
				100M,
				0M,
				DateTime.ParseExact("2023-12-03T03:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				null,
				null,
				null,
				null,
				null,
				null)
		};

		var actual = await sut.QueryAsync(query).ToArrayAsync();

		CollectionAssert.AreEqual(expected, actual);
	}
}
