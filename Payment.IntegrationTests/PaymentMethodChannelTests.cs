using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

public class PaymentMethodChannelTests
{
	[Test]
	public async Task Create_PaymentMethodChannel()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var paymentId = Guid.NewGuid();
		var methodId = Guid.NewGuid();
		var channelId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var actual = await sut.CreatePaymentMethodChannelAsync(paymentId, methodId, channelId);

		var pmc = await dbContext.PaymentMethodChannels.FindAsync(actual);

		Assert.AreEqual(paymentId, pmc.PaymentId);
		Assert.AreEqual(methodId, pmc.MethodId);
		Assert.AreEqual(channelId, pmc.ChannelId);
	}

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

	[Test]
	public async Task CreatePaymentMethodChannel_GetPaymentMethodChannel_Should_Success()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeMessageSender));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		_ = await sut.GetAllPaymentMethodChannelsAsync().ToArrayAsync();

		var paymentId = Guid.NewGuid();
		var methodId = Guid.NewGuid();
		var channelId = Guid.NewGuid();

		var pmcId = await sut.CreatePaymentMethodChannelAsync(paymentId, methodId, channelId);

		var actual = await sut.GetAllPaymentMethodChannelsAsync()
			.Where(pmc => pmc.Id == pmcId)
			.FirstOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(paymentId, actual.PaymentId);
		Assert.AreEqual(methodId, actual.MethodId);
		Assert.AreEqual(channelId, actual.ChannelId);
	}

	[Test]
	public async Task Delete_PaymentMethodChannel()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var pmcId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		await dbContext.PaymentMethodChannels.AddAsync(new PaymentMethodChannel
		{
			Id = pmcId,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			ChannelId = Guid.NewGuid()
		});

		await dbContext.SaveChangesAsync();

		dbContext.ChangeTracker.Clear();

		await sut.DeletePaymentMethodChannelAsync(pmcId);
	}

	[Test]
	public async Task DeletePaymentMethodChannel_GetPaymentMethodChannel_Should_Success()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeMessageSender));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var pmcId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		await dbContext.PaymentMethodChannels.AddAsync(new PaymentMethodChannel
		{
			Id = pmcId,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			ChannelId = Guid.NewGuid()
		});

		await dbContext.SaveChangesAsync();

		dbContext.ChangeTracker.Clear();

		_ = await sut.GetAllPaymentMethodChannelsAsync().ToArrayAsync();

		await sut.DeletePaymentMethodChannelAsync(pmcId);

		var actual = await sut.GetAllPaymentMethodChannelsAsync()
			.Where(pmc => pmc.Id == pmcId)
			.FirstOrDefaultAsync();

		Assert.IsNull(actual);
	}
}
