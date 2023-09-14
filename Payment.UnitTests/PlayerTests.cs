using NSubstitute;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Players;

namespace Payment.UnitTests;

public class PlayerTests
{
	[Test]
	public async Task 取得玩家金流總覽()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetPaymentOverviewQueryService(fakePaymentRepository);

		var query = new GetPaymentOverviewQuery("test1");

		_ = await sut.GetAsync(query);

		_ = fakePaymentRepository.Received(1)
			.GetPlayerPaymentOverviewAsync(Arg.Is(query.PlayerCode));
	}

	[Test]
	public async Task 查詢當日玩家已通過存款和已支付提款金額()
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
