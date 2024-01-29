using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Players;

namespace Payment.UnitTests;

public class PlayerTests
{
	[Test]
	public async Task GetPaymentOverviewQueryService_取得玩家金流總覽()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeStatisticsRepository = Substitute.For<IStatisticsRepository>();
		var fakeTimeProvider = Substitute.For<TimeProvider>();

		var sut = new GetPaymentOverviewQueryService(
			fakeTimeProvider,
			fakePaymentRepository,
			fakeStatisticsRepository);

		var query = new GetPaymentOverviewQuery("test1");
		var currentTime = DateTimeOffset.ParseExact("2024-01-29T15:27:00.0000000+08:00", "o", null);

		_ = fakeTimeProvider.GetUtcNow()
			.Returns(currentTime);

		_ = await sut.GetAsync(query);

		_ = fakePaymentRepository.Received(1)
			.GetPlayerPaymentOverviewAsync(Arg.Is(query.PlayerCode));
		_ = fakeStatisticsRepository.Received(1)
			.Query5minPaymentStatisticsAsync(
				Arg.Is(currentTime.FloorHourTime().UtcDateTime),
				Arg.Is(currentTime.UtcDateTime),
				Arg.Any<string[]>());
		_ = fakeStatisticsRepository.Received(1)
			.QueryHourPaymentStatisticsAsync(
				Arg.Is(DateTimeOffset.ParseExact("2024-01-29T00:00:00.0000000+08:00", "o", null).UtcDateTime),
				Arg.Is(currentTime.FloorHourTime().UtcDateTime),
				Arg.Any<string[]>());
	}

	[Test]
	public async Task GetCurrentDayTotalAmountQueryService_查詢當日玩家已通過存款和已支付提款金額()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetCurrentDayTotalAmountQueryService(fakePaymentRepository);

		var playerCodes = new string[] { "4uel5ke0uf3k", "4ukjwezjyxlq", "4ukxm6x4y8vf" };

		var query = new GetCurrentDayTotalAmountQuery(playerCodes);

		_ = await sut.QueryAsync(query).ToArrayAsync();

		_ = fakePaymentRepository
			.Received(1)
			.GetCurrentDayDepositWithdrawTotalAmountAsync(Arg.Is(playerCodes));
	}
}
