using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests;

public class PaymentMethodChannelTests
{
	[Test]
	public async Task CreatePayment_GetPayment_Should_Success()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeMessageSender));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var paymentCode = "Test";

		fakeMessageSender
			.When(fake => fake.PublishAsync(
				Arg.Is("tgs.payment.created"),
				Arg.Any<ReadOnlyMemory<byte>>(),
				Arg.Any<IEnumerable<MessageHeaderValue>>()))
			.Do(callInfo =>
			{
				var data = callInfo.Arg<ReadOnlyMemory<byte>>();

				var handler = (IMessageHandler)ActivatorUtilities.CreateInstance(
					web.Services,
					Tgs.Payment.Cache.MessageQueue.ProcessorTypes.PaymentCreatedEventHandlerType);

				handler.HandleAsync(data).GetAwaiter().GetResult();
			});

		_ = await sut.GetAllPaymentsAsync().ToArrayAsync();

		await sut.CreatePaymentAsync(new PaymentCreateInfo
		{
			Code = paymentCode
		});

		var actual = await sut.GetAllPaymentsAsync()
			.Where(p => p.Code == paymentCode)
			.FirstOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(paymentCode, actual.Code);
	}
}
