using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

public class PlayerQueryTests
{
	[Test]
	public async Task 查詢玩家帳戶總覽()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var playerCode = "tester1";

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var currentTime = DateTime.UtcNow;

		_ = await dbContext.PlayerPaymentOverviews.AddAsync(new PlayerPaymentOverview
		{
			PlayerCode = playerCode,
			FirstDepositAmount = 0,
			FirstDepositHandlingFee = 0,
			FirstDepositOn = currentTime,
			FirstWithdrawAmount = 0,
			FirstWithdrawHandlingFee = 0,
			FirstWithdrawOn = currentTime,
			RecordedBy = DateOnly.FromDateTime(currentTime),
			TotalDepositAmount = 0,
			TotalWithdrawAmount = 0
		});

		_ = await dbContext.SaveChangesAsync();

		var expected = new Tgs.Payment.Clients.PaymentOverview(
			0M,
			0M,
			currentTime,
			0M,
			0M,
			currentTime,
			DateOnly.FromDateTime(currentTime),
			0M,
			0M);

		var actual = await sut.GetPlayerPaymentOverviewAsync(playerCode);

		ClassicAssert.AreEqual(expected, actual);
	}

	[Test]
	public async Task 查詢玩家帳戶總覽_當資料庫中沒有資料時()
	{
		var fakeTimeProvider = Substitute.For<TimeProvider>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeTimeProvider));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var playerCode = "tester1";
		var currentTime = DateTimeOffset.ParseExact("2024-01-29T16:16:00.0000000+08:00", "o", null);

		_ = fakeTimeProvider.GetUtcNow()
			.Returns(currentTime);

		var expected = new Tgs.Payment.Clients.PaymentOverview(
			null,
			null,
			null,
			null,
			null,
			null,
			DateOnly.Parse("2024-1-29"),
			0M,
			0M);

		var actual = await sut.GetPlayerPaymentOverviewAsync(playerCode);

		ClassicAssert.AreEqual(expected, actual);
	}

	[Test]
	public async Task 查詢玩家帳戶總覽_還沒有統計出總覽資料_會從5分鐘及小時匯總資料運算出來()
	{
		var fakeTimeProvider = Substitute.For<TimeProvider>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeTimeProvider));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var playerCode = "tester1";
		var brandId = Guid.NewGuid();
		var currentTime = DateTimeOffset.ParseExact("2024-01-29T16:16:00.0000000+08:00", "o", null);

		_ = fakeTimeProvider.GetUtcNow()
			.Returns(currentTime);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.FiveMinutesPaymentStatistics.AddAsync(new FiveMinutesPaymentStatistic
		{
			TimePoint = DateTime.ParseExact("2024-01-29T16:10:00.0000000+08:00", "o", null).ToUniversalTime(),
			BrandId = brandId,
			UserId = playerCode,
			UserType = (int)Tgs.Payment.UserType.Player,
			DepositAmount = 100M,
			DepositHandlingFee = 0M,
			DepositCount = 1,
			FirstDepositAmount = 100M,
			FirstDepositHandlingFee = 0M,
			FirstDepositOn = DateTime.ParseExact("2024-01-29T16:10:00.0000000+08:00", "o", null).ToUniversalTime()
		});

		_ = await dbContext.HourPaymentStatistics.AddAsync(new HourPaymentStatistic
		{
			TimePoint = DateTime.ParseExact("2024-01-29T15:00:00.0000000+08:00", "o", null).ToUniversalTime(),
			BrandId = brandId,
			UserId = playerCode,
			UserType = (int)Tgs.Payment.UserType.Player,
			DepositAmount = 100M,
			DepositHandlingFee = 0M,
			DepositCount = 2,
			FirstDepositAmount = 50M,
			FirstDepositHandlingFee = 0M,
			FirstDepositOn = DateTime.ParseExact("2024-01-29T15:00:00.0000000+08:00", "o", null).ToUniversalTime()
		});

		_ = await dbContext.SaveChangesAsync();

		var expected = new Tgs.Payment.Clients.PaymentOverview(
			50M,
			0M,
			DateTime.ParseExact("2024-01-29T15:00:00.0000000+08:00", "o", null).ToUniversalTime(),
			null,
			null,
			null,
			DateOnly.Parse("2024-1-29"),
			200M,
			0M);

		var actual = await sut.GetPlayerPaymentOverviewAsync(playerCode);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public async Task 查詢玩家帳戶總覽_會加總即時資料()
	{
		var fakeTimeProvider = Substitute.For<TimeProvider>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeTimeProvider));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var playerCode = "tester1";
		var brandId = Guid.NewGuid();
		var currentTime = DateTimeOffset.ParseExact("2024-01-29T16:16:00.0000000+08:00", "o", null);

		_ = fakeTimeProvider.GetUtcNow()
			.Returns(currentTime);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.FiveMinutesPaymentStatistics.AddAsync(new FiveMinutesPaymentStatistic
		{
			TimePoint = DateTime.ParseExact("2024-01-29T16:10:00.0000000+08:00", "o", null).ToUniversalTime(),
			BrandId = brandId,
			UserId = playerCode,
			UserType = (int)Tgs.Payment.UserType.Player,
			DepositAmount = 100M,
			DepositHandlingFee = 0M,
			DepositCount = 1,
			FirstDepositAmount = 100M,
			FirstDepositHandlingFee = 0M,
			FirstDepositOn = DateTime.ParseExact("2024-01-29T16:10:00.0000000+08:00", "o", null).ToUniversalTime()
		});

		_ = await dbContext.HourPaymentStatistics.AddAsync(new HourPaymentStatistic
		{
			TimePoint = DateTime.ParseExact("2024-01-29T15:00:00.0000000+08:00", "o", null).ToUniversalTime(),
			BrandId = brandId,
			UserId = playerCode,
			UserType = (int)Tgs.Payment.UserType.Player,
			DepositAmount = 100M,
			DepositHandlingFee = 0M,
			DepositCount = 2,
			FirstDepositAmount = 50M,
			FirstDepositHandlingFee = 0M,
			FirstDepositOn = DateTime.ParseExact("2024-01-29T15:00:00.0000000+08:00", "o", null).ToUniversalTime()
		});

		_ = await dbContext.PlayerPaymentOverviews.AddAsync(new PlayerPaymentOverview
		{
			PlayerCode = playerCode,
			FirstDepositAmount = 300M,
			FirstDepositHandlingFee = 0,
			FirstDepositOn = DateTime.ParseExact("2024-01-28T15:00:00.0000000+08:00", "o", null).ToUniversalTime(),
			FirstWithdrawAmount = 100M,
			FirstWithdrawHandlingFee = 0,
			FirstWithdrawOn = DateTime.ParseExact("2024-01-28T15:00:00.0000000+08:00", "o", null).ToUniversalTime(),
			RecordedBy = DateOnly.Parse("2024-1-28"),
			TotalDepositAmount = 100M,
			TotalWithdrawAmount = 100M
		});

		_ = await dbContext.SaveChangesAsync();

		var expected = new Tgs.Payment.Clients.PaymentOverview(
			300M,
			0M,
			DateTime.ParseExact("2024-01-28T15:00:00.0000000+08:00", "o", null).ToUniversalTime(),
			100M,
			0,
			DateTime.ParseExact("2024-01-28T15:00:00.0000000+08:00", "o", null).ToUniversalTime(),
			DateOnly.Parse("2024-1-28"),
			300M,
			100M);

		var actual = await sut.GetPlayerPaymentOverviewAsync(playerCode);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public async Task 查詢當日玩家已通過存款與已支付提款金額()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(services
			=> services.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var playerCodes = new string[] { "playerCode", "playerCode1" };

		_ = fakePaymentRepository
			.GetCurrentDayDepositWithdrawTotalAmountAsync(
				Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(playerCodes)),
				Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.TopTenDepositWithdrawInfo[]
			{
				new()
				{
					PlayerCode = "playerCode",
					DepositTotalAmount = 888888m,
					WithdrawTotalAmount = 100m
				},
				new()
				{
					PlayerCode = "playerCode1",
					DepositTotalAmount = 16888m,
					WithdrawTotalAmount = 100m
				}
			}.ToAsyncEnumerable());

		var actual = await sut.GetCurrentDayDepositWithdrawTotalAmountAsync(playerCodes).ToArrayAsync();

		ClassicAssert.AreEqual(2, actual.Length);
	}
}
