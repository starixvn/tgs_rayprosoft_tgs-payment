using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Payments;

namespace Payment.UnitTests
{
	[TestFixture]
	public class PaymentTests
	{
		[Test]
		public async Task Can_Create_Payment()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new CreatePaymentCommandService(fakePaymentRepository);

			var command = new CreatePaymentCommand("PaymentCode", null, null);

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.CreatePaymentAsync(Arg.Any<PaymentCreateInfo>());
		}

		[Test]
		public async Task Can_Create_PaymentChannel()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new CreatePaymentChannelCommandService(fakePaymentRepository);

			var command = new CreatePaymentChannelCommand("ChannelCode");

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.CreatePaymentChannelAsync(Arg.Is(command.ChannelCode));
		}

		[Test]
		public async Task Can_Create_PaymentMethod()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new CreatePaymentMethodCommandService(fakePaymentRepository);

			var command = new CreatePaymentMethodCommand("MethodCode", "Bank");

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.CreatePaymentMethodAsync(Arg.Is(command.Code), Arg.Is(command.BankType));
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

		[Test]
		public async Task User_Can_Get_PaymentInfo()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new GetPaymentQueryService(fakePaymentRepository);

			var paymentId = Guid.NewGuid();

			var query = new GetPaymentQuery(paymentId);

			_ = await sut.GetAsync(query);

			_ = fakePaymentRepository.Received(1)
				.GetPaymentAsync(Arg.Is(paymentId));
		}
	}
}
