using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.Clients;

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
			.GetBrandPaymentFeeSettingsAsync(
				Arg.Is(brandPaymentId),
				Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.BrandPaymentFeeInfo(10m, true));

		var actual = await sut.CalculateHandlingFeeAsync(brandPaymentId, amount);

		Assert.AreEqual(10m, actual);
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
			.GetBrandPaymentFeeSettingsAsync(
				Arg.Is(brandPaymentId),
				Arg.Any<CancellationToken>())
			.Returns(new Tgs.Payment.BrandPaymentFeeInfo(10m, false));

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
}
