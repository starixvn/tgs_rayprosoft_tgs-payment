using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework.Legacy;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Payment.IntegrationTests;

public class DepositTests
{
	[Test]
	public async Task Brand指定日期存款成功率()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services
			=> services.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var reportDate = DateOnly.FromDateTime(DateTime.Now);

		_ = fakePaymentRepository
			.CountCurrentDayDepositFormsStatusAsync(brandId, Arg.Any<CancellationToken>())
			.Returns(new DepositFormsStatusCount
			{
				Total = 3,
				Approved = 2,
				Reject = 0,
			});

		var actual = await sut.GetBrandSuccessRateByReportDateAsync(brandId, reportDate);

		ClassicAssert.AreEqual(67m, actual);
	}

	[Test]
	public async Task Brand當日存款成功率()
	{
		var fakeService = Substitute.For<IGetService<CurrentDaySuccessRateQuery, decimal>>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services
			=> services.AddSingleton(fakeService));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

		_ = fakeService
			.GetAsync(new CurrentDaySuccessRateQuery(brandId), Arg.Any<CancellationToken>())
			.Returns(67m);

		var actual = await sut.GetCurrentDayDepositSuccessRateAsync(brandId);

		ClassicAssert.AreEqual(67m, actual);
	}

	[Test]
	public async Task 查詢當日前十大存款玩家()
	{
		var fakeService = Substitute.For<IQueryService<GetCurrentDayTopTenDepositPlayersQuery, Tgs.Payment.TopTenDepositWithdrawInfo>>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services
			=> services.AddSingleton(fakeService));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

		_ = fakeService
			.QueryAsync(new GetCurrentDayTopTenDepositPlayersQuery(brandId), Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.TopTenDepositWithdrawInfo[] {
				new()
				{
					PlayerCode = "playerCode1",
					DepositTotalAmount = 16888m,
					WithdrawTotalAmount = 200m
				}
			}.ToAsyncEnumerable());

		var actual = await sut.GetCurrentDayTopTenDepositPlayersAsync(brandId).ToArrayAsync();

		ClassicAssert.AreEqual(1, actual.Length);
		ClassicAssert.AreEqual("playerCode1", actual[0].PlayerCode);
		ClassicAssert.AreEqual(16888m, actual[0].DepositTotalAmount);
		ClassicAssert.AreEqual(200m, actual[0].WithdrawTotalAmount);
	}
}
