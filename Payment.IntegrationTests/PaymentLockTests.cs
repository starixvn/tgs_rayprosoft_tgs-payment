using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests;

public class PaymentLockTests
{
	[Test]
	public async Task LockPayment()
	{
		var fakePaymentRateLimitRepository = Substitute.For<IPaymentRateLimitRepository>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRateLimitRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var formId = Guid.NewGuid();
		var paymentCode = "Test";
		var playerCode = "tester1";

		var expected = formId;

		_ = fakePaymentRateLimitRepository
			.LockPaymentAsync(
				Arg.Is(formId),
				Arg.Is(paymentCode),
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(formId);

		var actual = await sut.LockDepositPaymentAsync(formId, paymentCode, playerCode);

		Assert.AreEqual(expected, actual);
	}

	[Test]
	public async Task UnLockPayment()
	{
		var fakePaymentRateLimitRepository = Substitute.For<IPaymentRateLimitRepository>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakePaymentRateLimitRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var formId = Guid.NewGuid();
		var paymentCode = "Test";
		var playerCode = "tester1";

		await sut.UnlockDepositPaymentAsync(formId, paymentCode, playerCode);
	}
}
