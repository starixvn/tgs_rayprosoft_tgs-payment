using System;
using System.Collections.Generic;
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

		[Test]
		public void Can_List_BrandPayment_By_BrandId()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new BrandPaymentQueryService(fakePaymentRepository);

			var query = new BrandPaymentQuery(Guid.NewGuid());

			_ = sut.QueryAsync(query);

			_ = fakePaymentRepository.Received(1)
				.GetBrandPaymentsAsync(Arg.Is(query.BrandId));
		}

		[Test]
		public void Can_List_BrandPaymentAcl_By_PlayerCode_And_BrandPaymentIds()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new BrandPaymentAclQueryService(fakePaymentRepository);

			var query = new BrandPaymentAclQuery(
				"ThisIsPlayerCode",
				new[] {
					Guid.NewGuid()
				});

			_ = sut.QueryAsync(query);

			_ = fakePaymentRepository.Received(1)
				.GetBrandPaymentAclsAsync(
					Arg.Any<IEnumerable<Guid>>(),
					Arg.Is(query.PlayerCode));
		}

		[Test]
		public void Can_List_BrandPaymentVip_By_BrandPaymentIds()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new BrandPaymentVipQueryService(fakePaymentRepository);

			var query = new BrandPaymentVipQuery(
				new[] {
					Guid.NewGuid()
				});

			_ = sut.QueryAsync(query);

			_ = fakePaymentRepository.Received(1)
				.GetBrandPaymentVipListAsync(
					Arg.Any<IEnumerable<Guid>>());
		}
	}
}
