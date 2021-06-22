using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices;

namespace Payment.UnitTests
{
	[TestFixture]
	public class PaymentTests
	{
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
		public void User_Can_Get_All_PaymentMethodInfos()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new AllPaymentMethodsQueryService(fakePaymentRepository);

			_ = sut.QueryAsync(new AllPaymentMethodsQuery());

			_ = fakePaymentRepository.Received(1)
				.GetAllPaymentMethodsAsync();
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
	}
}
