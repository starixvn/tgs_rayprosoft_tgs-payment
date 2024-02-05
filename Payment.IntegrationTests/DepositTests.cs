using Microsoft.Extensions.DependencyInjection;
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

	[Test]
	public async Task 整合測試_取得存款單內容()
	{
		var fakeService = Substitute.For<IGetService<GetDepositFormDetailByFormIdQuery, Tgs.Payment.DepositFormInfo?>>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeService));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var formId = Guid.NewGuid();

		_ = fakeService.GetAsync(
			Arg.Is<GetDepositFormDetailByFormIdQuery>(query => query.FormId == formId),
			Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.DepositFormInfo(
				formId,
				"test1",
				"test1",
				Guid.NewGuid(),
				Guid.NewGuid(),
				"aaa",
				Guid.NewGuid(),
				1000M,
				null,
				null,
				Tgs.Payment.PaymentFlowState.Approved,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				10M));

		var actual = await sut.GetDepositFormAsync(formId);

		Assert.That(actual, Is.Not.Null);
		Assert.That(actual!.Id, Is.EqualTo(formId));
		Assert.That(actual.ApplyAmount, Is.EqualTo(990M));
	}

	[Test]
	public async Task 整合測試_查詢存款單列表()
	{
		var fakeCountService = Substitute.For<IGetService<DepositFormQuery, int>>();
		var fakeQueryService = Substitute.For<IQueryService<DepositFormQuery, Tgs.Payment.DepositFormInfo>>();
		var fakeTotalAmountService = Substitute.For<IGetService<DepositFormQuery, decimal>>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeCountService)
			.AddSingleton(fakeQueryService)
			.AddSingleton(fakeTotalAmountService));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var conditions = new Tgs.Payment.Clients.DepositFormQueryConditions
		{
			BrandId = Guid.NewGuid()
		};
		var paging = new Paging();

		var formId = Guid.NewGuid();

		_ = fakeCountService.GetAsync(
			Arg.Any<DepositFormQuery>(),
			Arg.Any<CancellationToken>())
			.Returns(10);
		_ = fakeTotalAmountService.GetAsync(
			Arg.Any<DepositFormQuery>(),
			Arg.Any<CancellationToken>())
			.Returns(1000M);
		_ = fakeQueryService.QueryAsync(Arg.Any<DepositFormQuery>(), Arg.Any<CancellationToken>())
			.Returns(new[]
			{
				new Tgs.Payment.DepositFormInfo(
				formId,
				"test1",
				"test1",
				conditions.BrandId,
				Guid.NewGuid(),
				"aaa",
				Guid.NewGuid(),
				1000M,
				null,
				null,
				Tgs.Payment.PaymentFlowState.Approved,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				10M)
			}.ToAsyncEnumerable());

		var actual = await sut.QueryDepositFormsAsync(conditions, paging);

		Assert.That(actual.TotalCount, Is.EqualTo(10));
		Assert.That(actual.Sum, Is.EqualTo(1000M));
		Assert.That(actual.Items.Count, Is.EqualTo(1));
		Assert.That(actual.Items.First().ApplyAmount, Is.EqualTo(990M));
	}
}
