using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.BrandPayments;

namespace Payment.UnitTests;

[TestFixture]
public class BrandPaymentTests
{
	[Test]
	public async Task Can_Calculate_BrandPayment_Fee_By_BrandPaymentId()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CalculateHandlingFeeCommandService(fakePaymentRepository);

		var query = new CalculateHandlingFeeCommand(Guid.NewGuid(), 1000m);

		_ = fakePaymentRepository
			.GetBrandPaymentFeeSettingsAsync(query.BrandPaymentId)
			.Returns(new BrandPaymentFeeInfo(10m, false));

		var actual = await sut.GetAsync(query);

		Assert.AreEqual(0m, actual);
	}

	[Test]
	public async Task Can_Calculate_BrandPayment_Fee_By_BrandPayment_Id()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CalculateHandlingFeeCommandService(fakePaymentRepository);

		var query = new CalculateHandlingFeeCommand(Guid.NewGuid(), 1000m);

		_ = fakePaymentRepository
			.GetBrandPaymentFeeSettingsAsync(query.BrandPaymentId)
			.Returns(new BrandPaymentFeeInfo(10m, true));

		var actual = await sut.GetAsync(query);

		Assert.AreEqual(10m, actual);
	}

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
	public async Task Can_Get_BrandPayment_Fee_Setting_By_BrandPaymentId()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetBrandPaymentFeeQueryService(fakePaymentRepository);

		var query = new GetBrandPaymentFeeQuery(Guid.NewGuid());

		_ = await sut.GetAsync(query);

		_ = fakePaymentRepository.Received(1)
			.GetBrandPaymentFeeSettingsAsync(Arg.Is(query.Id));
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
	public void Can_List_BrandPayment_Designated_Users_By_BrandPaymentId()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new BrandPaymentDesignatedUserQueryService(fakePaymentRepository);

		var brandPaymentId = Guid.NewGuid();

		var query = new BrandPaymentDesignatedUserQuery(brandPaymentId);

		_ = sut.QueryAsync(query);

		_ = fakePaymentRepository.Received(1)
			.GetBrandPaymentDesignatedUsersAsync(
				Arg.Any<Guid>());
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
	public void Can_List_BrandPayments_By_BrandId_Or_TransferType_Or_Enabled_Or_Keyword()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new BrandPaymentsQueryService(fakePaymentRepository);

		var query = new BrandPaymentsQuery(
			Guid.NewGuid(),
			TransferType.Deposit,
			true,
			null);

		_ = sut.QueryAsync(query);

		_ = fakePaymentRepository.Received(1)
			.QueryBrandPaymentsAsync(
				Arg.Is(query.BrandId),
				Arg.Is(query.TransferType),
				Arg.Is(query.Enabled),
				Arg.Is(query.Keyword));
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
