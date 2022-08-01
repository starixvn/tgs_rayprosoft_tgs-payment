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

	[Test]
	public async Task CreatePaymentChannel_GetPaymentChannels_Should_Success()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeMessageSender));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var channelCode = "Test";

		fakeMessageSender
			.When(fake => fake.PublishAsync(
				Arg.Is("tgs.payment-channel.created"),
				Arg.Any<ReadOnlyMemory<byte>>(),
				Arg.Any<IEnumerable<MessageHeaderValue>>()))
			.Do(callInfo =>
			{
				var data = callInfo.Arg<ReadOnlyMemory<byte>>();

				var handler = (IMessageHandler)ActivatorUtilities.CreateInstance(
					web.Services,
					Tgs.Payment.Cache.MessageQueue.ProcessorTypes.PaymentChannelCreatedEventHandlerType);

				handler.HandleAsync(data).GetAwaiter().GetResult();
			});

		_ = await sut.GetAllPaymentChannelsAsync().ToArrayAsync();

		await sut.CreatePaymentChannelAsync(channelCode);

		var actual = await sut.GetAllPaymentChannelsAsync()
			.Where(c => c.Name == channelCode)
			.FirstOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(channelCode, actual.Name);
	}

	[Test]
	public async Task CreatePaymentMethod_GetPaymentMethods_Should_Success()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeMessageSender));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var createInfo = new PaymentMethodCreateInfo
		{
			BankType = "Bank",
			Code = "Test"
		};

		fakeMessageSender
			.When(fake => fake.PublishAsync(
				Arg.Is("tgs.payment-method.created"),
				Arg.Any<ReadOnlyMemory<byte>>(),
				Arg.Any<IEnumerable<MessageHeaderValue>>()))
			.Do(callInfo =>
			{
				var data = callInfo.Arg<ReadOnlyMemory<byte>>();

				var handler = (IMessageHandler)ActivatorUtilities.CreateInstance(
					web.Services,
					Tgs.Payment.Cache.MessageQueue.ProcessorTypes.PaymentMethodCreatedEventHandlerType);

				handler.HandleAsync(data).GetAwaiter().GetResult();
			});

		_ = await sut.GetAllPaymentMethodsAsync().ToArrayAsync();

		await sut.CreatePaymentMethodAsync(createInfo);

		var actual = await sut.GetAllPaymentMethodsAsync()
			.Where(m => m.Name == "Test")
			.FirstOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(createInfo.Code, actual.Name);
	}
}
