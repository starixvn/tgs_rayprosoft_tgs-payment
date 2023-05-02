using NSubstitute;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Agents;

namespace Payment.UnitTests;

public class AgentTests
{
	[Test]
	public async Task 取得代理資訊()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetAgentInfoQueryService(fakePaymentRepository);

		var query = new GetAgentInfoQuery(Guid.NewGuid());

		_ = await sut.GetAsync(query);

		_ = fakePaymentRepository.Received(1)
			.GetAgentInfoByAgentIdAsync(Arg.Is(query.AgentId));
	}
}
