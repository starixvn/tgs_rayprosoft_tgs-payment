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
}
