using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

public class BrandPaymentTests
{
	[Test]
	public async Task Calculate_Handling_Fee()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandPaymentId = Guid.NewGuid();
		var amount = 1000m;

		var expected = brandPaymentId;

		_ = fakePaymentRepository
			.GetBrandPaymentFeeSettingAsync(
				Arg.Is(brandPaymentId),
				Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.BrandPaymentFeeInfo(brandPaymentId, 10m, true));

		var actual = await sut.CalculateHandlingFeeAsync(brandPaymentId, amount);

		Assert.AreEqual(10m, actual);
	}

	[Test]
	public async Task Get_Brand_Auto_Withdraw_Setting()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();

		_ = fakePaymentRepository
			.GetBrandAutoWithdrawSettingAsync(
				Arg.Is(brandId),
				Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.AutoWithdrawInfo(true, 168m, new Guid[] { Guid.NewGuid() }));

		var actual = await sut.GetBrandAutoWithdrawSettingAsync(brandId);

		Assert.AreEqual(168m, actual.AmountUpperLimit);
		Assert.AreEqual(true, actual.EnableAutoWithdraw);
		Assert.AreEqual(1, actual.BrandPaymentIds.Count());
	}

	[Test]
	public async Task Get_Brand_Auto_Withdraw_Setting2()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();

		var actual = await sut.GetBrandAutoWithdrawSettingAsync(brandId);

		Assert.IsNull(actual);
	}

	[Test]
	public async Task Get_BrandPayment_Fee_Settings()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandPaymentId = Guid.NewGuid();

		_ = fakePaymentRepository
			.GetBrandPaymentFeeSettingAsync(
				Arg.Is(brandPaymentId),
				Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.BrandPaymentFeeInfo(brandPaymentId, 10m, false));

		var actual = await sut.GetBrandPaymentFeeSettingsAsync(brandPaymentId);

		Assert.AreEqual(10m, actual.HandlingFeeRateTenPercentile);
		Assert.AreEqual(false, actual.ReducePlayerAmount);
	}

	[Test]
	public async Task Get_BrandPayment_Info_By_BrandPament_Id()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandPaymentId = Guid.NewGuid();

		_ = fakePaymentRepository
			.GetBrandPaymentByIdAsync(
				Arg.Is(brandPaymentId),
				Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.BrandPaymentInfo(
				brandPaymentId,
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				"channelName",
				"channelCode",
				"{}",
				new Tgs.Payment.ValueRange<decimal> { From = 0, To = 1000 },
				true,
				false,
				false,
				new Tgs.Payment.ValueRange<decimal> { From = 0, To = 1000 },
				Tgs.Payment.ConditionMatchType.MatchOne,
				Tgs.Payment.TransferType.Deposit,
				true,
				0,
				Guid.NewGuid()));

		var actual = await sut.GetBrandPaymentByIdAsync(brandPaymentId);

		Assert.AreEqual(brandPaymentId, actual.Id);
	}

	[Test]
	public async Task Get_BrandPayments_Fee_Settings()
	{
		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandPaymentIds = new Guid[] {
			Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8"),
			Guid.Parse("072a90e2-8912-4a78-3ccc-08d9262ce1a5")
		};

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		await dbContext.BrandPayments.AddRangeAsync(
			new BrandPayment
			{
				Id = Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8"),
				HandlingFeeRateTenPercentile = 0M,
				ReducePlayerAmount = false,
				ChannelName = "This is channel name",
				CreatedByUsername = "username",
				ExtendedData = "{}",
				UpdatedByUsername = "username",
			},
			new BrandPayment
			{
				Id = Guid.Parse("072a90e2-8912-4a78-3ccc-08d9262ce1a5"),
				HandlingFeeRateTenPercentile = 10M,
				ReducePlayerAmount = true,
				ChannelName = "This is channel name2",
				CreatedByUsername = "username2",
				ExtendedData = "{}",
				UpdatedByUsername = "username2",
			});

		_ = await dbContext.SaveChangesAsync();

		var actual = await sut.GetBrandPaymentsFeeSettingsAsync(brandPaymentIds).ToArrayAsync();

		var expected = new Tgs.Payment.Clients.BrandPaymentFeeInfo[] {
			new Tgs.Payment.Clients.BrandPaymentFeeInfo()
			{
				HandlingFeeRateTenPercentile = 0m,
				ReducePlayerAmount = false,
				BrandPaymentId = Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8")
			},
			new Tgs.Payment.Clients.BrandPaymentFeeInfo()
			{
				HandlingFeeRateTenPercentile =10m,
				ReducePlayerAmount = true,
				BrandPaymentId = Guid.Parse("072a90e2-8912-4a78-3ccc-08d9262ce1a5")
			}
		};

		Assert.AreEqual(2, actual.Length);
		CollectionAssert.AreEquivalent(expected, actual);
	}

	[Test]
	public async Task Update_BrandPayment_Extened_Data_By_BrandPament_Id()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandPaymentId = Guid.NewGuid();

		_ = fakePaymentRepository
			.GetBrandPaymentByIdAsync(
				Arg.Is(brandPaymentId),
				Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.BrandPaymentInfo(
				brandPaymentId,
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				"channelName",
				"channelCode",
				"{}",
				new Tgs.Payment.ValueRange<decimal> { From = 0, To = 1000 },
				true,
				false,
				false,
				new Tgs.Payment.ValueRange<decimal> { From = 0, To = 1000 },
				Tgs.Payment.ConditionMatchType.MatchOne,
				Tgs.Payment.TransferType.Deposit,
				true,
				0,
				Guid.NewGuid()));

		await sut.UpdateBrandPaymentExtendedDataAsync(brandPaymentId, JsonDocument.Parse("{\"name\":\"Test\"}"));
	}

	[Test]
	public async Task Upsert_Brand_Auto_Withdraw_By_Brand_Id()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();

		await sut.UpdateBrandAutoWithdrawAsync(
			brandId,
			true,
			168M,
			new Guid[] { Guid.Parse("018a2193-4282-0000-0001-07e7e07dc945") },
			"186f897e-56d2-47e6-b349-08d920ec59b6");

		_ = fakePaymentRepository
			.Received(1)
			.CreateBrandAutoWithdrawEventAsync(
				Arg.Any<Guid>(),
				Arg.Any<Guid>(),
				Arg.Any<JsonDocument>(),
				Arg.Any<string>(),
				Arg.Any<CancellationToken>());

		_ = fakePaymentRepository
			.Received(1)
			.UpdateBrandAutoWithdrawSnapshotAsync(
				Arg.Any<Tgs.Payment.AutoWithdrawInfo>(),
				Arg.Any<Guid>(),
				Arg.Any<Guid>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task 查詢Brand指定日期存提差_有統計資料()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeBrandNetDepositRepository = Substitute.For<IBrandNetDepositRepository>();

		await using TestPaymentApplication web = TestSiteHelper
			.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRepository)
			.AddSingleton(fakeBrandNetDepositRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();
		var reportDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

		_ = fakePaymentRepository
			.GetBrandNetDepositAsync(Arg.Is(brandId), Arg.Is(reportDate), Arg.Any<CancellationToken>())
			.Returns(168000m);

		var actual = await sut.GetBrandReportDateNetDepositAsync(brandId, reportDate);

		Assert.AreEqual(168000m, actual);
	}

	[Test]
	public async Task 查詢Brand指定日期存提差_統計資料未出來時_返回即時資料()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeBrandNetDepositRepository = Substitute.For<IBrandNetDepositRepository>();

		await using TestPaymentApplication web = TestSiteHelper
			.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRepository)
			.AddSingleton(fakeBrandNetDepositRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();
		var reportDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

		_ = fakeBrandNetDepositRepository
			.GetBrandCurrentDayNetDepositAsync(Arg.Is(brandId))
			.Returns(168000m);

		var actual = await sut.GetBrandReportDateNetDepositAsync(brandId, reportDate);

		Assert.AreEqual(168000m, actual);
	}

	[Test]
	public async Task 查詢Brand當日存提差()
	{
		var fakeBrandNetDepositRepository = Substitute.For<IBrandNetDepositRepository>();

		await using TestPaymentApplication web = TestSiteHelper
			.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeBrandNetDepositRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();

		_ = fakeBrandNetDepositRepository
			.GetBrandCurrentDayNetDepositAsync(Arg.Is(brandId))
			.Returns(168000m);

		var actual = await sut.GetBrandCurrentDayNetDepositAsync(brandId);

		Assert.AreEqual(168000m, actual);
	}
}
