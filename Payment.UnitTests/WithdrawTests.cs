using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.Paging;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.BrandPayments;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Payment.UnitTests;

[TestFixture]
public class WithdrawTests
{
	[Test]
	public async Task BrandId_Not_Match_Can_Not_Get_WithdrawForm()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetWithdrawFormDetailQueryService(fakePaymentRepository);

		var query = new GetWithdrawFormDetailQuery(Guid.NewGuid(), Guid.NewGuid());

		_ = fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Any<Guid>())
			.Returns(new WithdrawFormInfo(
				query.FormId,
				  "PlayerCode",
				  UserType.Player,
				  "UserName",
				  Guid.NewGuid(),
				  1000M,
				  null,
				  PaymentFlowState.WaitApprove,
				  null,
				  null,
				  null,
				  "GPIW211028160001",
				  Guid.NewGuid(),
				  DateTimeOffset.Now,
				  DateTimeOffset.Now,
				  null,
				  null,
				  null,
				  HandlingFee: 0M,
				  null,
				  null));

		var actual = await sut.GetAsync(query);

		Assert.IsNull(actual);
	}

	[Test]
	public async Task Can_Create_Agent_WithdrawForm()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var brandId = Guid.NewGuid();
		var command = new CreateWithdrawFormCommand(
			Guid.NewGuid().ToString(),
			"TestAgent",
			UserType.Agent,
			brandId,
			Guid.NewGuid(),
			1000M,
			null);

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var agentBankInfo = new AgentBankInfo(
			command.BankInfoId,
			Guid.Parse(command.UserId),
			command.BrandId,
			Guid.NewGuid(),
			"Bank",
			"{}");
		_ = fakePaymentRepository.GetAgentBankInfoByIdAsync(Arg.Is(command.BankInfoId))
			.Returns(agentBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var actual = await sut.GetAsync(command);

		Assert.AreNotEqual(Guid.Empty, actual);

		var expectedSnapshot = new WithdrawFormSnapshot(
			actual,
			command.BrandId,
			command.UserId,
			command.UserName,
			command.UserType,
			command.Amount,
			null,
			0M,
			null,
			PaymentFlowState.Init,
			null,
			command.Note,
			command.BankInfoId,
			JsonSerializer.Serialize(new WithdrawBankInfo<JsonDocument>(
				agentBankInfo.Id,
				agentBankInfo.PaymentChannelId,
				agentBankInfo.BankType,
				JsonDocument.Parse(agentBankInfo.Detail)), PaymentUtility.SerializerOptions),
			null,
			null,
			orderNo,
			null,
			currentTime.UtcDateTime,
			null,
			null,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.Received(1)
			.GetBrandInfoByIdAsync(Arg.Is(brandId));

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(actual),
				Arg.Is(EventAction.Create),
				Arg.Any<JsonDocument>(),
				Arg.Any<string>());

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormCreatedEventAsync(Arg.Is(actual), Arg.Any<Guid>());
	}

	[Test]
	public async Task Can_Create_Player_Withdraw_Form()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var brandId = Guid.NewGuid();

		var command = new CreateWithdrawFormCommand(
			"ThisIsPlayerCode",
			"TestUser",
			UserType.Player,
			brandId,
			Guid.NewGuid(),
			1000M,
			"ThisIsPlayerNote");

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		var playerBankInfo = new PlayerBankInfo(
			command.BankInfoId,
			command.UserId,
			command.BrandId,
			Guid.NewGuid(),
			"Bank",
			JsonDocument.Parse("{}"),
			false);
		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(command.BankInfoId))
			.Returns(playerBankInfo);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var orderNo = "TT11234567";
		_ = fakeTakeNumberStore.GetWithdrawFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var actual = await sut.GetAsync(command);

		Assert.AreNotEqual(Guid.Empty, actual);

		var expectedSnapshot = new WithdrawFormSnapshot(
			actual,
			command.BrandId,
			command.UserId,
			command.UserName,
			command.UserType,
			command.Amount,
			null,
			0M,
			null,
			PaymentFlowState.Init,
			null,
			command.Note,
			command.BankInfoId,
			JsonSerializer.Serialize(new WithdrawBankInfo<JsonDocument>(
				playerBankInfo.Id,
				playerBankInfo.PaymentChannelId,
				playerBankInfo.BankType,
				playerBankInfo.Detail), PaymentUtility.SerializerOptions),
			null,
			null,
			orderNo,
			null,
			currentTime.UtcDateTime,
			null,
			null,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.Received(1)
			.GetBrandInfoByIdAsync(Arg.Is(brandId));

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(actual),
				Arg.Is(EventAction.Create),
				Arg.Any<JsonDocument>(),
				Arg.Any<string>());

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormCreatedEventAsync(Arg.Is(actual), Arg.Any<Guid>());
	}

	[Test]
	public async Task Can_Get_One_WithdrawForm()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetWithdrawFormDetailQueryService(fakePaymentRepository);

		var query = new GetWithdrawFormDetailQuery(Guid.NewGuid(), Guid.NewGuid());

		_ = fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Is(query.FormId))
			.Returns(new WithdrawFormInfo(
				query.FormId,
				  "PlayerCode",
				  UserType.Player,
				  "UserName",
				  query.BrandId,
				  1000M,
				  null,
				  PaymentFlowState.WaitApprove,
				  null,
				  null,
				  null,
				  "GPIW211028160001",
				  Guid.NewGuid(),
				  DateTimeOffset.Now,
				  DateTimeOffset.Now,
				  null,
				  null,
				  null,
				  HandlingFee: 0M,
				  Guid.NewGuid(),
				  "ChannelName",
				  Guid.NewGuid(),
				  "BankInfo",
				  null,
				  null,
				  null,
				  "PlayerNote"));

		var actual = await sut.GetAsync(query);

		Assert.IsNotNull(actual);
		Assert.AreEqual("PlayerNote", actual.Note);
	}

	[Test]
	public async Task Can_Get_One_WithdrawForm_By_FormId()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetWithdrawFormDetailByFormIdQueryService(fakePaymentRepository);

		var query = new GetWithdrawFormDetailByFormIdQuery(Guid.NewGuid());

		_ = fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Is(query.FormId))
			.Returns(new WithdrawFormInfo(
				query.FormId,
				  "PlayerCode",
				  UserType.Player,
				  "UserName",
				  Guid.NewGuid(),
				  1000M,
				  null,
				  PaymentFlowState.WaitApprove,
				  null,
				  null,
				  null,
				  "GPIW211028160001",
				  Guid.NewGuid(),
				  DateTimeOffset.Now,
				  DateTimeOffset.Now,
				  null,
				  null,
				  null,
				  HandlingFee: 0M,
				  Guid.NewGuid(),
				  "ChannelName",
				  Guid.NewGuid(),
				  "BankInfo",
				  null,
				  null,
				  null,
				  "PlayerNote"));

		var actual = await sut.GetAsync(query);

		Assert.IsNotNull(actual);
		Assert.AreEqual("PlayerNote", actual.Note);
	}

	[Test]
	public void Can_Query_WithdrawForms()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new WithdrawFormQueryService(fakePaymentRepository);

		var query = new WithdrawFormQuery(
			Guid.NewGuid(),
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			new[] {
				PaymentFlowState.WaitPayment,
				PaymentFlowState.WaitApprove,
				PaymentFlowState.Approved,
				PaymentFlowState.Reject
			},
			Array.Empty<Guid>(),
			null,
			null,
			null,
			null,
			new Paging<WithdrawFormInfo>());

		_ = sut.QueryAsync(query);

		_ = fakePaymentRepository.Received(1)
			.QueryWithdrawFormsAsync(
				Arg.Any<WithdrawFormQueryConditions>(),
				Arg.Any<Paging<WithdrawFormInfo>>());
	}

	[Test]
	public async Task Can_Reject_Form_In_WaitApprove()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeWithdrawProcessorFactory = Substitute.For<IWithdrawProcessorFactory>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeCalculateService = Substitute.For<IGetService<CalculateHandlingFeeCommand, decimal>>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SignOffWithdrawFormCommandService(
			fakePaymentRepository,
			fakeWithdrawProcessorFactory,
			fakePaymentClient,
			fakeCalculateService,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SignOffWithdrawFormCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ReviewState.Reject,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		var withdrawForm = new WithdrawFormSnapshot(
			command.FormId,
			Guid.NewGuid(),
			"ThisIsPlayerCode",
			"UserName",
			UserType.Player,
			1000M,
			null,
			0M,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			Guid.NewGuid(),
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			DateTime.UtcNow,
			null,
			null,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.GetCurrentWithdrawFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(withdrawForm);

		_ = fakePaymentRepository
			.GetBrandPaymentByIdAsync(Arg.Is(command.BrandPaymentId.Value))
			.Returns(new BrandPaymentInfo(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				"TestChannel",
				"TestChannelCode",
				"{}",
				new ValueRange<decimal>
				{
					From = 0M,
					To = 1000M
				},
				true,
				false,
				false,
				new ValueRange<decimal>
				{
					From = 0M,
					To = decimal.MaxValue
				},
				ConditionMatchType.MatchOne,
				TransferType.Withdraw,
				true,
				0,
				Guid.NewGuid()));

		_ = fakePaymentRepository.GetPaymentAsync(Arg.Any<Guid>())
			.Returns(new PaymentInfo(Guid.NewGuid(), "LeePay"));

		_ = fakeWithdrawProcessorFactory.GetWithdrawProcessor(Arg.Any<string>())
			.Returns(Substitute.For<IWithdrawProcessor>());

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = new WithdrawFormSnapshot(
			command.FormId,
			withdrawForm.BrandId,
			withdrawForm.UserId,
			withdrawForm.UserName,
			withdrawForm.UserType,
			1000M,
			null,
			0M,
			null,
			PaymentFlowState.Reject,
			null,
			null,
			withdrawForm.BankInfoId,
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			withdrawForm.CreatedOn,
			command.ReviewUserId,
			command.ReviewUserName,
			currentTime.UtcDateTime,
			null,
			null,
			null);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.Review),
				Arg.Any<JsonDocument>(),
				Arg.Is(command.ReviewUserId));

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task Can_Reject_Form_In_WaitPaid()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeWithdrawProcessorFactory = Substitute.For<IWithdrawProcessorFactory>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeCalculateService = Substitute.For<IGetService<CalculateHandlingFeeCommand, decimal>>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SignOffWithdrawFormCommandService(
			fakePaymentRepository,
			fakeWithdrawProcessorFactory,
			fakePaymentClient,
			fakeCalculateService,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SignOffWithdrawFormCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ReviewState.Reject,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		var withdrawForm = new WithdrawFormSnapshot(
			command.FormId,
			Guid.NewGuid(),
			"ThisIsPlayerCode",
			"UserName",
			UserType.Player,
			1000M,
			null,
			0M,
			null,
			PaymentFlowState.Approved,
			null,
			null,
			Guid.NewGuid(),
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			DateTime.UtcNow,
			Guid.NewGuid().ToString(),
			"Approver",
			DateTime.UtcNow,
			null,
			null,
			null);

		_ = fakePaymentRepository.GetCurrentWithdrawFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(withdrawForm);

		_ = fakePaymentRepository
			.GetBrandPaymentByIdAsync(Arg.Is(command.BrandPaymentId.Value))
			.Returns(new BrandPaymentInfo(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				"TestChannel",
				"TestChannelCode",
				"{}",
				new ValueRange<decimal>
				{
					From = 0M,
					To = 1000M
				},
				true,
				false,
				false,
				new ValueRange<decimal>
				{
					From = 0M,
					To = decimal.MaxValue
				},
				ConditionMatchType.MatchOne,
				TransferType.Withdraw,
				true,
				0,
				Guid.NewGuid()));

		_ = fakePaymentRepository.GetPaymentAsync(Arg.Any<Guid>())
			.Returns(new PaymentInfo(Guid.NewGuid(), "LeePay"));

		_ = fakeWithdrawProcessorFactory.GetWithdrawProcessor(Arg.Any<string>())
			.Returns(Substitute.For<IWithdrawProcessor>());

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = withdrawForm with
		{
			Status = PaymentFlowState.Reject,
			ReviewedUserId = command.ReviewUserId,
			ReviewedUserName = command.ReviewUserName,
			ReviewedOn = currentTime.UtcDateTime
		};

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.Review),
				Arg.Any<JsonDocument>(),
				Arg.Is(command.ReviewUserId));

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task Can_SignOff_Form()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeWithdrawProcessorFactory = Substitute.For<IWithdrawProcessorFactory>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeCalculateService = Substitute.For<IGetService<CalculateHandlingFeeCommand, decimal>>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SignOffWithdrawFormCommandService(
			fakePaymentRepository,
			fakeWithdrawProcessorFactory,
			fakePaymentClient,
			fakeCalculateService,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SignOffWithdrawFormCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ReviewState.Approve,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		var withdrawForm = new WithdrawFormSnapshot(
			command.FormId,
			Guid.NewGuid(),
			"ThisIsPlayerCode",
			"UserName",
			UserType.Player,
			1000M,
			null,
			0M,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			Guid.NewGuid(),
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			DateTime.UtcNow,
			null,
			null,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.GetCurrentWithdrawFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(withdrawForm);

		_ = fakePaymentRepository
			.GetBrandPaymentByIdAsync(Arg.Is(command.BrandPaymentId.Value))
			.Returns(new BrandPaymentInfo(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				"TestChannel",
				"TestChannelCode",
				"{}",
				new ValueRange<decimal>
				{
					From = 0M,
					To = 1000M
				},
				true,
				false,
				false,
				new ValueRange<decimal>
				{
					From = 0M,
					To = decimal.MaxValue
				},
				ConditionMatchType.MatchOne,
				TransferType.Withdraw,
				true,
				0,
				Guid.NewGuid()));

		_ = fakePaymentRepository.GetPaymentAsync(Arg.Any<Guid>())
			.Returns(new PaymentInfo(Guid.NewGuid(), "LeePay"));

		_ = fakeWithdrawProcessorFactory.GetWithdrawProcessor(Arg.Any<string>())
			.Returns(Substitute.For<IWithdrawProcessor>());

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = withdrawForm with
		{
			BrandPaymentId = command.BrandPaymentId,
			Status = PaymentFlowState.Approved,
			ReviewedUserId = command.ReviewUserId,
			ReviewedUserName = command.ReviewUserName,
			ReviewedOn = currentTime.UtcDateTime
		};

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.Review),
				Arg.Any<JsonDocument>(),
				Arg.Is(command.ReviewUserId));

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task Can_Submit_WithdrawForm_Status()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SubmitWithdrawFormStatusCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SubmitWithdrawFormStatusCommand(
			Guid.NewGuid(),
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			null,
			null);

		var withdrawForm = new WithdrawFormSnapshot(
			command.FormId,
			Guid.NewGuid(),
			"PlayerCode",
			"UserName",
			UserType.Player,
			1000M,
			null,
			0M,
			Guid.NewGuid(),
			PaymentFlowState.Init,
			null,
			null,
			Guid.NewGuid(),
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			DateTime.UtcNow,
			null,
			null,
			null,
			null,
			null,
			null);
		_ = fakePaymentRepository.GetCurrentWithdrawFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(withdrawForm);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = withdrawForm with
		{
			Status = PaymentFlowState.WaitApprove
		};

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.ChangeStatus),
				Arg.Any<JsonDocument>(),
				Arg.Is(default(string)));

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public void Can_Sum_WithdrawForms()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CalculateWithdrawTotalAmountService(fakePaymentRepository);

		var query = new WithdrawFormQuery(
			Guid.NewGuid(),
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			new[] {
				PaymentFlowState.WaitPayment,
				PaymentFlowState.WaitApprove,
				PaymentFlowState.Approved,
				PaymentFlowState.Reject
			},
			Array.Empty<Guid>(),
			null,
			null,
			null,
			null,
			null);

		_ = sut.GetAsync(query);

		_ = fakePaymentRepository.Received(1)
			.CalculateWithdrawTotalAmountAsync(Arg.Any<WithdrawFormQueryConditions>());
	}

	[Test]
	public void Create_Agent_WithdrawForm_If_BankInfo_Not_Self_Will_Fail()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			 fakeSerialNumberGenerator);

		var brandId = Guid.NewGuid();

		var command = new CreateWithdrawFormCommand(
			Guid.NewGuid().ToString(),
			"TestAgent",
			UserType.Agent,
			brandId,
			Guid.NewGuid(),
			1000M,
			null);

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		_ = fakePaymentRepository.GetAgentBankInfoByIdAsync(Arg.Is(command.BankInfoId))
			.Returns(new AgentBankInfo(
				command.BankInfoId,
				Guid.NewGuid(),
				command.BrandId,
				Guid.NewGuid(),
				"Bank",
				"{}"));

		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.GetAsync(command));
	}

	[Test]
	public void Create_Player_Withdraw_Form_If_BankInfo_Not_Self_Will_Fail()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var brandId = Guid.NewGuid();

		var command = new CreateWithdrawFormCommand(
			"ThisIsPlayerCode",
			"TestUser",
			UserType.Player,
			brandId,
			Guid.NewGuid(),
			1000M,
			null);

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(command.BankInfoId))
			.Returns(new PlayerBankInfo(
				command.BankInfoId,
				"OtherPlayerCode",
				command.BrandId,
				Guid.NewGuid(),
				"Bank",
				JsonDocument.Parse("{}"),
				false));

		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.GetAsync(command));
	}

	[Test]
	public void Create_Withdraw_Form_With_Invalid_UserType_Will_Fail()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var brandId = Guid.NewGuid();

		var command = new CreateWithdrawFormCommand(
			"Unknown",
			"Unknown",
			UserType.None,
			brandId,
			Guid.NewGuid(),
			1000M,
			null);

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.GetAsync(command));
	}

	[Test]
	public async Task Submit_WithdrawForm_Paid()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SubmitWithdrawFormStatusCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SubmitWithdrawFormStatusCommand(
			Guid.NewGuid(),
			PaymentFlowState.Paid,
			null,
			null,
			null,
			"PaidUser",
			"PaidUserName");

		var withdrawForm = new WithdrawFormSnapshot(
			command.FormId,
			Guid.NewGuid(),
			"PlayerCode",
			"UserName",
			UserType.Player,
			1000M,
			null,
			0M,
			Guid.NewGuid(),
			PaymentFlowState.Approved,
			null,
			null,
			Guid.NewGuid(),
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			DateTime.UtcNow,
			Guid.NewGuid().ToString(),
			"Reviewer",
			DateTime.UtcNow,
			null,
			null,
			null);
		_ = fakePaymentRepository.GetCurrentWithdrawFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(withdrawForm);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = withdrawForm with
		{
			Status = PaymentFlowState.Paid,
			PaidUserId = command.PaidUserId,
			PaidUserName = command.PaidUserName,
			PaidOn = currentTime.UtcDateTime
		};

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.ChangeStatus),
				Arg.Any<JsonDocument>(),
				Arg.Is(default(string)));

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task Submit_WithdrawForm_With_Null_Status_Will_Not_Send_Emit_Status_Change_Event()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SubmitWithdrawFormStatusCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SubmitWithdrawFormStatusCommand(
			Guid.NewGuid(),
			Status: null,
			null,
			null,
			null,
			"PaidUser",
			"PaidUserName");

		var withdrawForm = new WithdrawFormSnapshot(
			command.FormId,
			Guid.NewGuid(),
			"PlayerCode",
			"UserName",
			UserType.Player,
			1000M,
			null,
			0M,
			Guid.NewGuid(),
			PaymentFlowState.Approved,
			null,
			null,
			Guid.NewGuid(),
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			DateTime.UtcNow,
			Guid.NewGuid().ToString(),
			"Reviewer",
			DateTime.UtcNow,
			null,
			null,
			null);
		_ = fakePaymentRepository.GetCurrentWithdrawFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(withdrawForm);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = withdrawForm with
		{
			Status = PaymentFlowState.Approved,
			PaidUserId = command.PaidUserId,
			PaidUserName = command.PaidUserName,
			PaidOn = null
		};

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.ChangeStatus),
				Arg.Any<JsonDocument>(),
				Arg.Is(default(string)));

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(0)
			.EmitWithdrawFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task 如果更新狀態為Paid_就會填寫PaidOn時間()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SubmitWithdrawFormStatusCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SubmitWithdrawFormStatusCommand(
			Guid.NewGuid(),
			PaymentFlowState.Paid,
			null,
			null,
			null,
			null,
			null);

		var withdrawForm = new WithdrawFormSnapshot(
			command.FormId,
			Guid.NewGuid(),
			"PlayerCode",
			"UserName",
			UserType.Player,
			1000M,
			null,
			0M,
			Guid.NewGuid(),
			PaymentFlowState.Approved,
			null,
			null,
			Guid.NewGuid(),
			"{}",
			null,
			null,
			"GPIW211028160001",
			null,
			DateTime.UtcNow,
			Guid.NewGuid().ToString(),
			"Reviewer",
			DateTime.UtcNow,
			null,
			null,
			null);
		_ = fakePaymentRepository.GetCurrentWithdrawFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(withdrawForm);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = withdrawForm with
		{
			Status = PaymentFlowState.Paid,
			PaidOn = currentTime.UtcDateTime
		};

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateWithdrawFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.ChangeStatus),
				Arg.Any<JsonDocument>(),
				Arg.Is(default(string)));

		_ = fakePaymentRepository.Received(1)
			.UpdateWithdrawFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task 站台存提差_加總已支付提款金額()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeCountRepository = Substitute.For<IBrandNetDepositRepository>();

		var sut = new CountWithdrawPaidCommandService(fakePaymentRepository, fakeCountRepository, NullLogger<CountWithdrawPaidCommandService>.Instance);

		var brandId = Guid.NewGuid();
		var formId = Guid.NewGuid();
		var amount = 168m;

		var withdrawForm = new WithdrawFormInfo(
			formId,
			"playerCode",
			UserType.Player,
			"userName",
			brandId,
			amount,
			null,
			PaymentFlowState.Paid,
			null,
			"reviewedUsername",
			DateTimeOffset.Now,
			null,
			Guid.NewGuid(),
			DateTimeOffset.Now,
			DateTimeOffset.Now,
			null,
			null,
			null,
			0m);

		_ = fakePaymentRepository
			.GetWithdrawFormByIdAsync(formId)
			.Returns(withdrawForm);

		await sut.ExecuteAsync(new CountWithdrawPaidCommand(brandId, formId, PaymentFlowState.Approved));

		_ = fakeCountRepository
			.Received(1)
			.CountBrandNetDepositAsync(Arg.Is(brandId), Arg.Is((double)-amount));
	}

	[Test]
	public async Task 站台存提差_只處理已支付提款單()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeCountRepository = Substitute.For<IBrandNetDepositRepository>();

		var sut = new CountWithdrawPaidCommandService(fakePaymentRepository, fakeCountRepository, NullLogger<CountWithdrawPaidCommandService>.Instance);

		await sut.ExecuteAsync(new CountWithdrawPaidCommand(Guid.NewGuid(), Guid.NewGuid(), PaymentFlowState.WaitApprove));

		_ = fakePaymentRepository
			.Received(0)
			.GetWithdrawFormByIdAsync(Arg.Any<Guid>());

		_ = fakeCountRepository
			.Received(0)
			.CountBrandNetDepositAsync(Arg.Any<Guid>(), Arg.Any<double>());
	}

	[Test]
	public async Task 提款自動出款_只處理玩家提款單()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSignOffWithdrawService = Substitute.For<ICommandService<SignOffWithdrawFormCommand>>();

		var sut = new AutoApproveWithdrawFormCommandService(
			fakePaymentRepository,
			fakeSignOffWithdrawService,
			NullLogger<AutoApproveWithdrawFormCommandService>.Instance);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var formId = Guid.Parse("38e8bd90-7eb6-4460-bdf3-a1c1336e8044");

		var brandPaymentId = Guid.Parse("07e50f08-e0ca-4679-b043-7cc6c48f9826");

		_ = fakePaymentRepository
			.GetBrandAutoWithdrawSettingAsync(brandId)
			.Returns(new AutoWithdrawInfo(true, 200m, new Guid[] { brandPaymentId }));

		var agentWithdrawForm = new WithdrawFormInfo(
			formId,
			UserId: "9fbc1b81-44ee-4f00-ba12-a13170c01024",
			UserType.Agent,
			"AgentUsername",
			brandId,
			168m,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			"GPIW230828000001",
			Guid.Parse("94278519-99b9-4844-b1a9-522b46b32f4e"),
			new DateTimeOffset(2023, 8, 28, 0, 0, 0, TimeSpan.FromHours(8d)),
			new DateTimeOffset(2023, 8, 28, 0, 0, 0, TimeSpan.FromHours(8d)),
			null,
			null,
			null,
			0m);

		_ = fakePaymentRepository
			.GetWithdrawFormByIdAsync(formId)
			.Returns(agentWithdrawForm);

		await sut.ExecuteAsync(new AutoApproveWithdtawFormCommand(brandId, formId));

		_ = fakePaymentRepository
			.Received(0)
			.GetBrandPaymentsAsync(brandId);
	}

	[Test]
	public async Task 提款自動出款_可選金流與設定金流交集為0()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSignOffWithdrawService = Substitute.For<ICommandService<SignOffWithdrawFormCommand>>();

		var sut = new AutoApproveWithdrawFormCommandService(
			fakePaymentRepository,
			fakeSignOffWithdrawService,
			NullLogger<AutoApproveWithdrawFormCommandService>.Instance);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var formId = Guid.Parse("38e8bd90-7eb6-4460-bdf3-a1c1336e8044");
		var playerCode = "4uel5ke0uf3k";
		var playerBankInfoId = Guid.Parse("94278519-99b9-4844-b1a9-522b46b32f4e");
		var paymentChannelId = Guid.Parse("9e0b2d0d-6e4e-4a31-b482-946d33565640");

		_ = fakePaymentRepository
			.GetBrandAutoWithdrawSettingAsync(brandId)
			.Returns(new AutoWithdrawInfo(true, 200m, new Guid[] { Guid.Parse("07e50f08-e0ca-4679-b043-7cc6c48f9826") }));

		var playerWithdrawForm = new WithdrawFormInfo(
			formId,
			UserId: playerCode,
			UserType.Player,
			"playerUsername",
			brandId,
			168m,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			"GPIW230828000001",
			BankInfoId: playerBankInfoId,
			new DateTimeOffset(2023, 8, 28, 0, 0, 0, TimeSpan.FromHours(8d)),
			new DateTimeOffset(2023, 8, 28, 0, 0, 0, TimeSpan.FromHours(8d)),
			null,
			null,
			null,
			0m,
			BankInfo: $$"""
			{
				"id": "{{playerBankInfoId}}",
				"detail": {
					"phone": "09889988998899",
					"username": "真真真"
				},
				"bankType": "ThirdParty",
				"paymentChannelId": "{{paymentChannelId}}"
			}
			""");

		_ = fakePaymentRepository
			.GetWithdrawFormByIdAsync(formId)
			.Returns(playerWithdrawForm);

		var brandPayment = new BrandPaymentInfo(
			Guid.Parse("f33fabaf-870d-4bc6-bec2-5184265fe043"),
			PaymentMethodChannelId: Guid.Parse("0a2bb387-7985-4000-953c-5529d1062f98"),
			PaymentId: Guid.Parse("de0833b8-7ac7-4ef1-9742-5487469ef90f"),
			MethodId: Guid.Parse("5501b945-9043-46b7-87b5-48330f002d0d"),
			ChannelId: paymentChannelId,
			ChannelName: "AlfaPay_ThirdParty_GCash直連",
			"GCash",
			"{}",
			new ValueRange<decimal> { From = 100m, To = 10000m },
			true,
			false,
			false,
			new ValueRange<decimal> { From = 0, To = 0 },
			ConditionMatchType.MatchAll,
			TransferType.Withdraw,
			Enabled: true,
			DisplayOrder: 0,
			brandId);

		_ = fakePaymentRepository
			.GetBrandPaymentsAsync(brandId)
			.Returns(new BrandPaymentInfo[] { brandPayment }.ToAsyncEnumerable());

		await sut.ExecuteAsync(new AutoApproveWithdtawFormCommand(brandId, formId));

		_ = fakeSignOffWithdrawService
			.Received(0)
			.ExecuteAsync(Arg.Any<SignOffWithdrawFormCommand>());
	}

	[Test]
	public async Task 提款自動出款_自動出款()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSignOffWithdrawService = Substitute.For<ICommandService<SignOffWithdrawFormCommand>>();

		var sut = new AutoApproveWithdrawFormCommandService(
			fakePaymentRepository,
			fakeSignOffWithdrawService,
			NullLogger<AutoApproveWithdrawFormCommandService>.Instance);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var formId = Guid.Parse("38e8bd90-7eb6-4460-bdf3-a1c1336e8044");
		var playerCode = "4uel5ke0uf3k";
		var playerBankInfoId = Guid.Parse("94278519-99b9-4844-b1a9-522b46b32f4e");
		var paymentChannelId = Guid.Parse("9e0b2d0d-6e4e-4a31-b482-946d33565640");

		var brandPaymentId = Guid.Parse("07e50f08-e0ca-4679-b043-7cc6c48f9826");

		_ = fakePaymentRepository
			.GetBrandAutoWithdrawSettingAsync(brandId)
			.Returns(new AutoWithdrawInfo(true, 200m, new Guid[] { brandPaymentId }));

		var playerWithdrawForm = new WithdrawFormInfo(
			formId,
			UserId: playerCode,
			UserType.Player,
			"playerUsername",
			brandId,
			100m,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			"GPIW230828000001",
			BankInfoId: playerBankInfoId,
			new DateTimeOffset(2023, 8, 28, 0, 0, 0, TimeSpan.FromHours(8d)),
			new DateTimeOffset(2023, 8, 28, 0, 0, 0, TimeSpan.FromHours(8d)),
			null,
			null,
			null,
			0m,
			BankInfo: $$"""
			{
				"id": "{{playerBankInfoId}}",
				"detail": {
					"phone": "09889988998899",
					"username": "真真真"
				},
				"bankType": "ThirdParty",
				"paymentChannelId": "{{paymentChannelId}}"
			}
			""");

		_ = fakePaymentRepository
			.GetWithdrawFormByIdAsync(formId)
			.Returns(playerWithdrawForm);

		var brandPayment = new BrandPaymentInfo(
			brandPaymentId,
			PaymentMethodChannelId: Guid.Parse("0a2bb387-7985-4000-953c-5529d1062f98"),
			PaymentId: Guid.Parse("de0833b8-7ac7-4ef1-9742-5487469ef90f"),
			MethodId: Guid.Parse("5501b945-9043-46b7-87b5-48330f002d0d"),
			ChannelId: paymentChannelId,
			ChannelName: "AlfaPay_ThirdParty_GCash直連",
			"GCash",
			"{}",
			new ValueRange<decimal> { From = 100m, To = 10000m },
			true,
			false,
			false,
			new ValueRange<decimal> { From = 0, To = 0 },
			ConditionMatchType.MatchAll,
			TransferType.Withdraw,
			Enabled: true,
			DisplayOrder: 0,
			brandId);

		_ = fakePaymentRepository
			.GetBrandPaymentsAsync(brandId)
			.Returns(new BrandPaymentInfo[] { brandPayment }.ToAsyncEnumerable());

		await sut.ExecuteAsync(new AutoApproveWithdtawFormCommand(brandId, formId));

		_ = fakeSignOffWithdrawService
			.Received(1)
			.ExecuteAsync(Arg.Any<SignOffWithdrawFormCommand>());
	}

	[Test]
	public async Task 提款自動出款_自動出款設定_未開啟自動出款()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSignOffWithdrawService = Substitute.For<ICommandService<SignOffWithdrawFormCommand>>();

		var sut = new AutoApproveWithdrawFormCommandService(
			fakePaymentRepository,
			fakeSignOffWithdrawService,
			NullLogger<AutoApproveWithdrawFormCommandService>.Instance);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var formId = Guid.Parse("38e8bd90-7eb6-4460-bdf3-a1c1336e8044");

		_ = fakePaymentRepository
			.GetBrandAutoWithdrawSettingAsync(brandId)
			.Returns(new AutoWithdrawInfo(false, 10m, Array.Empty<Guid>()));

		await sut.ExecuteAsync(new AutoApproveWithdtawFormCommand(brandId, formId));

		_ = fakePaymentRepository
			.Received(0)
			.GetWithdrawFormByIdAsync(formId);
	}

	[Test]
	public async Task 提款自動出款_自動出款設定_金流通道未設定()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSignOffWithdrawService = Substitute.For<ICommandService<SignOffWithdrawFormCommand>>();

		var sut = new AutoApproveWithdrawFormCommandService(
			fakePaymentRepository,
			fakeSignOffWithdrawService,
			NullLogger<AutoApproveWithdrawFormCommandService>.Instance);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var formId = Guid.Parse("38e8bd90-7eb6-4460-bdf3-a1c1336e8044");

		_ = fakePaymentRepository
			.GetBrandAutoWithdrawSettingAsync(brandId)
			.Returns(new AutoWithdrawInfo(true, 10m, Array.Empty<Guid>()));

		await sut.ExecuteAsync(new AutoApproveWithdtawFormCommand(brandId, formId));

		_ = fakePaymentRepository
			.Received(0)
			.GetWithdrawFormByIdAsync(formId);
	}

	[Test]
	public async Task 提款自動出款_找不到自動出款設定檔()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSignOffWithdrawService = Substitute.For<ICommandService<SignOffWithdrawFormCommand>>();

		var sut = new AutoApproveWithdrawFormCommandService(
			fakePaymentRepository,
			fakeSignOffWithdrawService,
			NullLogger<AutoApproveWithdrawFormCommandService>.Instance);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var formId = Guid.Parse("38e8bd90-7eb6-4460-bdf3-a1c1336e8044");

		await sut.ExecuteAsync(new AutoApproveWithdtawFormCommand(brandId, formId));

		_ = fakePaymentRepository
			.Received(0)
			.GetWithdrawFormByIdAsync(formId);
	}

	[Test]
	public async Task 提款自動出款_找不到提款單()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSignOffWithdrawService = Substitute.For<ICommandService<SignOffWithdrawFormCommand>>();

		var sut = new AutoApproveWithdrawFormCommandService(
			fakePaymentRepository,
			fakeSignOffWithdrawService,
			NullLogger<AutoApproveWithdrawFormCommandService>.Instance);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var formId = Guid.Parse("38e8bd90-7eb6-4460-bdf3-a1c1336e8044");

		_ = fakePaymentRepository
			.GetBrandAutoWithdrawSettingAsync(brandId)
			.Returns(new AutoWithdrawInfo(true, 100m, new Guid[] { Guid.NewGuid() }));

		await sut.ExecuteAsync(new AutoApproveWithdtawFormCommand(brandId, formId));

		_ = fakePaymentRepository
			.Received(0)
			.GetBrandPaymentsAsync(Arg.Any<Guid>());
	}

	[Test]
	public async Task 提款自動出款_提款金額需小於等於設定金額()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeSignOffWithdrawService = Substitute.For<ICommandService<SignOffWithdrawFormCommand>>();

		var sut = new AutoApproveWithdrawFormCommandService(
			fakePaymentRepository,
			fakeSignOffWithdrawService,
			NullLogger<AutoApproveWithdrawFormCommandService>.Instance);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var formId = Guid.Parse("38e8bd90-7eb6-4460-bdf3-a1c1336e8044");
		var playerCode = "4uel5ke0uf3k";
		var playerBankInfoId = Guid.Parse("94278519-99b9-4844-b1a9-522b46b32f4e");
		var paymentChannelId = Guid.Parse("9e0b2d0d-6e4e-4a31-b482-946d33565640");

		_ = fakePaymentRepository
			.GetBrandAutoWithdrawSettingAsync(brandId)
			.Returns(new AutoWithdrawInfo(true, 100m, new Guid[] { Guid.NewGuid() }));

		var playerWithdrawForm = new WithdrawFormInfo(
			formId,
			UserId: playerCode,
			UserType.Player,
			"playerUsername",
			brandId,
			101m,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			"GPIW230828000001",
			BankInfoId: playerBankInfoId,
			new DateTimeOffset(2023, 8, 28, 0, 0, 0, TimeSpan.FromHours(8d)),
			new DateTimeOffset(2023, 8, 28, 0, 0, 0, TimeSpan.FromHours(8d)),
			null,
			null,
			null,
			0m,
			BankInfo: $$"""
			{
				"id": "{{playerBankInfoId}}",
				"detail": {
					"phone": "09889988998899",
					"username": "真真真"
				},
				"bankType": "ThirdParty",
				"paymentChannelId": "{{paymentChannelId}}"
			}
			""");

		_ = fakePaymentRepository
			.GetWithdrawFormByIdAsync(formId)
			.Returns(playerWithdrawForm);

		await sut.ExecuteAsync(new AutoApproveWithdtawFormCommand(brandId, formId));

		_ = fakePaymentRepository
			.Received(0)
			.GetBrandPaymentsAsync(brandId);
	}
}
;
