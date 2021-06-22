using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices;

namespace Payment.UnitTests
{
	[TestFixture]
	public class BrandPaymentTests
	{
		[Test]
		public async Task Can_Get_BrandPayment_By_Id()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new GetBrandPaymentQueryService(fakePaymentRepository);

			var query = new GetBrandPaymentQuery(Guid.NewGuid());

			_ = await sut.GetAsync(query);

			_ = fakePaymentRepository.Received(1)
				.GetBrandPaymentByIdAsync(Arg.Is(query.Id));
		}
	}
}
