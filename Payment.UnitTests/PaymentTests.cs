using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.Payment.Clients;

namespace Payment.UnitTests;

[TestFixture]
public class PaymentTests
{
	[Test]
	public async Task Can_Create_Payment()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new CreatePaymentCommandService(fakePaymentRepository, fakePaymentClient);

		var command = new CreatePaymentCommand("PaymentCode", null, null);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreatePaymentAsync(Arg.Any<PaymentCreateInfo>());
		_ = fakePaymentClient.Received(1)
			.EmitPaymentCreatedEventAsync(Arg.Is(command.Code));
	}

	[Test]
	public async Task Can_Create_PaymentChannel()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new CreatePaymentChannelCommandService(fakePaymentRepository, fakePaymentClient);

		var command = new CreatePaymentChannelCommand("ChannelCode");

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreatePaymentChannelAsync(Arg.Is(command.ChannelCode));
		_ = fakePaymentClient.Received(1)
			.EmitPaymentChannelCreatedEventAsync(Arg.Is(command.ChannelCode));
	}

	[Test]
	public async Task Can_Create_PaymentMethod()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new CreatePaymentMethodCommandService(fakePaymentRepository, fakePaymentClient);

		var command = new CreatePaymentMethodCommand("MethodCode", "Bank");

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreatePaymentMethodAsync(Arg.Is(command.Code), Arg.Is(command.BankType));
		_ = fakePaymentClient.Received(1)
			.EmitPaymentMethodCreatedEventAsync(Arg.Is(command.Code));
	}

	[Test]
	public async Task Can_Create_PaymentMethodChannel()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new CreatePaymentMethodChannelCommandService(fakePaymentRepository, fakePaymentClient);

		var command = new CreatePaymentMethodChannelCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid());

		_ = await sut.GetAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreatePaymentMethodChannelAsync(
				Arg.Is(command.PaymentId),
				Arg.Is(command.MethodId),
				Arg.Is(command.ChannelId));
		_ = fakePaymentClient.Received(1)
			.EmitPaymentMethodChannelChangedEventAsync(Arg.Any<Guid>());
	}

	[Test]
	public async Task Can_Delete_PaymentMethodChannel()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new DeletePaymentMethodChannelCommandService(fakePaymentRepository, fakePaymentClient);

		var command = new DeletePaymentMethodChannelCommand(Guid.NewGuid());

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.DeletePaymentMethodChannelAsync(Arg.Is(command.PaymentMethodChannelId));
		_ = fakePaymentClient.Received(1)
			.EmitPaymentMethodChannelChangedEventAsync(Arg.Is(command.PaymentMethodChannelId));
	}

	[Test]
	public void User_Can_Get_All_PaymentChannelInfos()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new AllPaymentChannelsQueryService(fakePaymentRepository);

		_ = sut.QueryAsync(new AllPaymentChannelsQuery());

		_ = fakePaymentRepository.Received(1)
			.GetAllPaymentChannelsAsync();
	}

	[Test]
	public void User_Can_Get_All_PaymentMethodChannelInfos()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new AllPaymentMethodChannelsQueryService(fakePaymentRepository);

		_ = sut.QueryAsync(new AllPaymentMethodChannelsQuery());

		_ = fakePaymentRepository.Received(1)
			.GetAllPaymentMethodChannelsAsync();
	}

	[Test]
	public void User_Can_Get_All_PaymentMethodInfos()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new AllPaymentMethodsQueryService(fakePaymentRepository);

		_ = sut.QueryAsync(new AllPaymentMethodsQuery());

		_ = fakePaymentRepository.Received(1)
			.GetAllPaymentMethodsAsync();
	}

	[Test]
	public void User_Can_Get_All_PaymnetInfos()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetAllPaymentsQueryService(fakePaymentRepository);

		_ = sut.QueryAsync(new GetAllPaymentsQuery());

		_ = fakePaymentRepository.Received(1)
			.GetAllPaymentsAsync();
	}

	//[Test]
	//public async Task User_Can_Get_PaymentInfo()
	//{
	//	var fakeQueryService = Substitute.For<IQueryService<GetAllPaymentsQuery, PaymentInfo>>();

	//	var sut = new GetPaymentQueryService(fakeQueryService);

	//	var paymentId = Guid.NewGuid();

	//	var query = new GetPaymentQuery(paymentId);

	//	_ = fakeQueryService.QueryAsync(Arg.Any<GetAllPaymentsQuery>())
	//		.Returns(new[] {
	//			new PaymentInfo(Guid.NewGuid(), "Test1"),
	//			new PaymentInfo(paymentId, "Test2")
	//		}.ToAsyncEnumerable());

	//	var actual = await sut.GetAsync(query);

	//	ClassicAssert.AreEqual(paymentId, actual.Id);
	//	ClassicAssert.AreEqual("Test2", actual.Code);
	//}
}
