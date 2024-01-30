using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using Tgs.Payment.Repositories;
using Tgs.Payment.Repositories.Entities;

namespace PaymentRepositories.UnitTests;

public class StatisticsRepositoryTests
{
	[Test]
	public async Task StatisticsRepository_查詢每日金流統計()
	{
		var dbContextOptions = new DbContextOptionsBuilder<TgsContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning))
			.Options;

		var dbContext = new TgsContext(dbContextOptions);
		var sut = new StatisticsRepository(dbContext);

		var start = DateOnly.Parse("2024-1-15");
		var end = DateOnly.Parse("2024-1-20");

		await dbContext.PlayerDailyPaymentStatistics.AddRangeAsync(
			new PlayerDailyPaymentStatistic
			{
				ReportDate = DateOnly.Parse("2024-1-15"),
				PlayerCode = "test1"
			});

		_ = await dbContext.SaveChangesAsync();

		var actual = await sut.QueryDailyPaymentStatisticsAsync(
			start,
			end,
			null).ToArrayAsync();

		Assert.That(actual.Length, Is.EqualTo(1));
	}

	[Test]
	public async Task StatisticsRepository_查詢每日金流統計_傳入空陣列的話等於不過濾玩家()
	{
		var dbContextOptions = new DbContextOptionsBuilder<TgsContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning))
			.Options;

		var dbContext = new TgsContext(dbContextOptions);
		var sut = new StatisticsRepository(dbContext);

		var start = DateOnly.Parse("2024-1-15");
		var end = DateOnly.Parse("2024-1-20");

		await dbContext.PlayerDailyPaymentStatistics.AddRangeAsync(
			new PlayerDailyPaymentStatistic
			{
				ReportDate = DateOnly.Parse("2024-1-15"),
				PlayerCode = "test1"
			});

		_ = await dbContext.SaveChangesAsync();

		var actual = await sut.QueryDailyPaymentStatisticsAsync(
			start,
			end,
			[]).ToArrayAsync();

		Assert.That(actual.Length, Is.EqualTo(1));
	}

	[Test]
	public async Task StatisticsRepository_查詢每小時金流統計()
	{
		var dbContextOptions = new DbContextOptionsBuilder<TgsContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning))
			.Options;

		var dbContext = new TgsContext(dbContextOptions);
		var sut = new StatisticsRepository(dbContext);

		var start = DateTime.ParseExact("2024-01-15T00:00:00.0000000+08:00", "o", null).ToUniversalTime();
		var end = DateTime.ParseExact("2024-01-16T00:00:00.0000000+08:00", "o", null).ToUniversalTime();
		var userIds = new[] { "test1" };

		await dbContext.HourPaymentStatistics.AddRangeAsync(
			new HourPaymentStatistic
			{
				TimePoint = DateTime.ParseExact("2024-01-15T01:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				BrandId = Guid.NewGuid(),
				UserId = "test1",
				UserType = 1
			});

		_ = await dbContext.SaveChangesAsync();

		var actual = await sut.QueryHourPaymentStatisticsAsync(
			start,
			end,
			userIds).ToArrayAsync();

		Assert.That(actual.Length, Is.EqualTo(1));
	}

	[Test]
	public async Task StatisticsRepository_查詢每小時金流統計_傳入空陣列等於不過濾玩家()
	{
		var dbContextOptions = new DbContextOptionsBuilder<TgsContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning))
			.Options;

		var dbContext = new TgsContext(dbContextOptions);
		var sut = new StatisticsRepository(dbContext);

		var start = DateTime.ParseExact("2024-01-15T00:00:00.0000000+08:00", "o", null).ToUniversalTime();
		var end = DateTime.ParseExact("2024-01-16T00:00:00.0000000+08:00", "o", null).ToUniversalTime();

		await dbContext.HourPaymentStatistics.AddRangeAsync(
			new HourPaymentStatistic
			{
				TimePoint = DateTime.ParseExact("2024-01-15T01:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				BrandId = Guid.NewGuid(),
				UserId = "test1",
				UserType = 1
			});

		_ = await dbContext.SaveChangesAsync();

		var actual = await sut.QueryHourPaymentStatisticsAsync(
			start,
			end,
			[]).ToArrayAsync();

		Assert.That(actual.Length, Is.EqualTo(1));
	}

	[Test]
	public async Task StatisticsRepository_查詢5分鐘金流統計()
	{
		var dbContextOptions = new DbContextOptionsBuilder<TgsContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning))
			.Options;

		var dbContext = new TgsContext(dbContextOptions);
		var sut = new StatisticsRepository(dbContext);

		var start = DateTime.ParseExact("2024-01-15T00:00:00.0000000+08:00", "o", null).ToUniversalTime();
		var end = DateTime.ParseExact("2024-01-15T01:00:00.0000000+08:00", "o", null).ToUniversalTime();
		var userIds = new[] { "test1" };

		await dbContext.FiveMinutesPaymentStatistics.AddRangeAsync(
			new FiveMinutesPaymentStatistic
			{
				TimePoint = DateTime.ParseExact("2024-01-15T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				BrandId = Guid.NewGuid(),
				UserId = "test1",
				UserType = 1
			});

		_ = await dbContext.SaveChangesAsync();

		var actual = await sut.Query5minPaymentStatisticsAsync(
			start,
			end,
			userIds).ToArrayAsync();

		Assert.That(actual.Length, Is.EqualTo(1));
	}

	[Test]
	public async Task StatisticsRepository_查詢5分鐘金流統計_傳入空陣列等於不過濾玩家()
	{
		var dbContextOptions = new DbContextOptionsBuilder<TgsContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning))
			.Options;

		var dbContext = new TgsContext(dbContextOptions);
		var sut = new StatisticsRepository(dbContext);

		var start = DateTime.ParseExact("2024-01-15T00:00:00.0000000+08:00", "o", null).ToUniversalTime();
		var end = DateTime.ParseExact("2024-01-15T01:00:00.0000000+08:00", "o", null).ToUniversalTime();

		await dbContext.FiveMinutesPaymentStatistics.AddRangeAsync(
			new FiveMinutesPaymentStatistic
			{
				TimePoint = DateTime.ParseExact("2024-01-15T00:00:00.0000000+08:00", "o", null).ToUniversalTime(),
				BrandId = Guid.NewGuid(),
				UserId = "test1",
				UserType = 1
			});

		_ = await dbContext.SaveChangesAsync();

		var actual = await sut.Query5minPaymentStatisticsAsync(
			start,
			end,
			[]).ToArrayAsync();

		Assert.That(actual.Length, Is.EqualTo(1));
	}
}
