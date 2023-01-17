using Microsoft.Extensions.DependencyInjection;
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

		Assert.AreEqual(expected, actual);
	}

	[Test]
	public async Task 查詢玩家帳戶總覽_當資料庫中沒有資料時()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var playerCode = "tester1";

		var expected = new Tgs.Payment.Clients.PaymentOverview(
			null,
			null,
			null,
			null,
			null,
			null,
			DateOnly.MinValue,
			0M,
			0M);

		var actual = await sut.GetPlayerPaymentOverviewAsync(playerCode);

		Assert.AreEqual(expected, actual);
	}
}
