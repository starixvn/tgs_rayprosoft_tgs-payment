using System.Text.Json;
using NSubstitute;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.BrandPayments;
using Tgs.SerialNumberGenerator;

namespace Payment.UnitTests;

[TestFixture]
public class BrandPaymentTests
{
	[Test]
	public async Task Can_Add_Brand_Auto_Withdraw_Setting()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var brandId = Guid.NewGuid();
		var eventId = Guid.NewGuid();

		var command = new UpdateBrandAutoWithdrawCommand(
			brandId,
			true,
			168M,
			new Guid[] { Guid.Parse("018a2193-4282-0000-0001-07e7e07dc945") },
			"userId");

		var sut = new UpdateBrandAutoWithdrawCommandService(fakePaymentRepository, fakeSystemClock, fakeSerialNumberGenerator);

		_ = fakeSerialNumberGenerator
			.GenerateAsync()
			.Returns(eventId);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository
			.Received(1)
			.CreateBrandAutoWithdrawEventAsync(
				Arg.Is(eventId),
				Arg.Is(brandId),
				Arg.Any<JsonDocument>(),
				Arg.Is(command.UserId));

		_ = fakePaymentRepository
			.Received(1)
			.UpdateBrandAutoWithdrawSnapshotAsync(
				Arg.Any<AutoWithdrawInfo>(),
				Arg.Is(brandId),
				Arg.Is(eventId),
				Arg.Any<DateTime>());
	}

	[Test]
	public async Task Can_Calculate_BrandPayment_Fee_By_BrandPayment_Id()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CalculateHandlingFeeCommandService(fakePaymentRepository);

		var brandPaymentId = Guid.NewGuid();
		var query = new CalculateHandlingFeeCommand(brandPaymentId, 1000m);

		_ = fakePaymentRepository
			.GetBrandPaymentFeeSettingAsync(query.BrandPaymentId)
			.Returns(new BrandPaymentFeeInfo(brandPaymentId, 10m, true));

		var actual = await sut.GetAsync(query);

		Assert.AreEqual(10m, actual);
	}

	[Test]
	public async Task Can_Calculate_BrandPayment_Fee_By_BrandPaymentId()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CalculateHandlingFeeCommandService(fakePaymentRepository);

		var brandPaymentId = Guid.NewGuid();
		var query = new CalculateHandlingFeeCommand(brandPaymentId, 1000m);

		_ = fakePaymentRepository
			.GetBrandPaymentFeeSettingAsync(query.BrandPaymentId)
			.Returns(new BrandPaymentFeeInfo(brandPaymentId, 10m, false));

		var actual = await sut.GetAsync(query);

		Assert.AreEqual(0m, actual);
	}

	[Test]
	public void Can_Get_Brand_Auto_Withdraw_Setting_By_BrandId()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetBrandAutoWithdrawQueryService(fakePaymentRepository);

		var query = new GetBrandAutoWithdrawQuery(Guid.NewGuid());

		_ = sut.GetAsync(query);

		_ = fakePaymentRepository
			.Received(1)
			.GetBrandAutoWithdrawSettingAsync(Arg.Is(query.BrandId));
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
			.GetBrandPaymentFeeSettingAsync(Arg.Is(query.Id));
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

	[Test]
	public async Task Can_Update_Brand_Auto_Withdraw_Setting()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var brandId = Guid.NewGuid();
		var eventId = Guid.NewGuid();

		var command = new UpdateBrandAutoWithdrawCommand(
			brandId,
			true,
			1680M,
			new Guid[] { Guid.Parse("95e7c419-127e-46fd-a322-a29ddaac6276") },
			"userId");

		var sut = new UpdateBrandAutoWithdrawCommandService(fakePaymentRepository, fakeSystemClock, fakeSerialNumberGenerator);

		_ = fakeSerialNumberGenerator
			.GenerateAsync()
			.Returns(eventId);

		_ = fakePaymentRepository
			.GetBrandAutoWithdrawSettingAsync(brandId)
			.Returns(new AutoWithdrawInfo(true, 168M, Array.Empty<Guid>()));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository
			.Received(1)
			.CreateBrandAutoWithdrawEventAsync(
				Arg.Is(eventId),
				Arg.Is(brandId),
				Arg.Any<JsonDocument>(),
				Arg.Is(command.UserId));

		_ = fakePaymentRepository
			.Received(1)
			.UpdateBrandAutoWithdrawSnapshotAsync(
				Arg.Any<AutoWithdrawInfo>(),
				Arg.Is(brandId),
				Arg.Is(eventId),
				Arg.Any<DateTime>());
	}
}
