using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Payment.IntegrationTests;

public class DepositTests
{
	[Test]
	public async Task 存款成功率()
	{
		var fakeService = Substitute.For<IGetService<SuccessRateQuery, decimal>>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services
			=> services.AddSingleton(fakeService));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

		_ = fakeService
			.GetAsync(new SuccessRateQuery(brandId), Arg.Any<CancellationToken>())
			.Returns(67m);

		var actual = await sut.GetDepositSuccessRateAsync(brandId);

		Assert.AreEqual(67m, actual);
	}
}
