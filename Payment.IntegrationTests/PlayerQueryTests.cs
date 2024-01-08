using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
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

		Assert.AreEqual(2, actual.Length);
	}
}
