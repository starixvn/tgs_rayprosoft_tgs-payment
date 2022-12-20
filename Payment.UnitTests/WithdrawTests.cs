using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
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

		var optionsAccessor = new OptionsWrapper<PaymentOptions>(new PaymentOptions());

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			optionsAccessor,
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

		await sut.ExecuteAsync(command);

		var actual = await command.ReturnFormIdSource.Task;

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
				JsonDocument.Parse(agentBankInfo.Detail)), optionsAccessor.Value.JsonSerializerOptions),
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
				Arg.Is(DepositFormAction.Create),
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

		var optionsAccessor = new OptionsWrapper<PaymentOptions>(new PaymentOptions());

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			optionsAccessor,
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

		await sut.ExecuteAsync(command);

		var actual = await command.ReturnFormIdSource.Task;

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
				playerBankInfo.Detail), optionsAccessor.Value.JsonSerializerOptions),
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
				Arg.Is(DepositFormAction.Create),
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

		var sut = new SignOffWithdrawFormCommandService(
			fakePaymentRepository,
			fakeWithdrawProcessorFactory,
			fakePaymentClient,
			fakeCalculateService);

		var command = new SignOffWithdrawFormCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ReviewState.Reject,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		_ = fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Any<Guid>())
			.Returns(new WithdrawFormInfo(
				command.FormId,
				"ThisIsPlayerCode",
				UserType.Player,
				"UserName",
				Guid.NewGuid(),
				1000M,
				null,
				PaymentFlowState.WaitApprove,
				null,
				"reviewedUserName",
				DateTime.Now,
				"GPIW211028160001",
				Guid.NewGuid(),
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				null,
				null,
				null,
				HandlingFee: 0M));

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

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SignOffWithdrawFormAsync(
				Arg.Is(command.FormId),
				Arg.Is(default(Guid?)),
				Arg.Any<string>(),
				Arg.Is(PaymentFlowState.Reject),
				Arg.Is(command.ReviewUserId),
				Arg.Is(command.ReviewUserName),
				Arg.Any<string>());

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

		var sut = new SignOffWithdrawFormCommandService(
			fakePaymentRepository,
			fakeWithdrawProcessorFactory,
			fakePaymentClient,
			fakeCalculateService);

		var command = new SignOffWithdrawFormCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ReviewState.Reject,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		_ = fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Any<Guid>())
			.Returns(new WithdrawFormInfo(
				command.FormId,
				"ThisIsPlayerCode",
				UserType.Player,
				"UserName",
				Guid.NewGuid(),
				1000M,
				null,
				PaymentFlowState.Approved,
				null,
				"reviewedUserName",
				DateTime.Now,
				"GPIW211028160001",
				Guid.NewGuid(),
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				null,
				null,
				null,
				HandlingFee: 0M));

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

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SignOffWithdrawFormAsync(
				Arg.Is(command.FormId),
				Arg.Is(default(Guid?)),
				Arg.Any<string>(),
				Arg.Is(PaymentFlowState.Reject),
				Arg.Is(command.ReviewUserId),
				Arg.Is(command.ReviewUserName),
				Arg.Any<string>());

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

		var sut = new SignOffWithdrawFormCommandService(
			fakePaymentRepository,
			fakeWithdrawProcessorFactory,
			fakePaymentClient,
			fakeCalculateService);

		var command = new SignOffWithdrawFormCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ReviewState.Approve,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		_ = fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Any<Guid>())
			.Returns(new WithdrawFormInfo(
				command.FormId,
				"ThisIsPlayerCode",
				UserType.Player,
				"UserName",
				Guid.NewGuid(),
				1000M,
				null,
				PaymentFlowState.WaitApprove,
				null,
				"reviewedUserName",
				DateTime.Now,
				"GPIW211028160001",
				Guid.NewGuid(),
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				null,
				null,
				null,
				HandlingFee: 0M));

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

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SignOffWithdrawFormAsync(
				Arg.Is(command.FormId),
				Arg.Is(command.BrandPaymentId),
				Arg.Any<string>(),
				Arg.Is(PaymentFlowState.Approved),
				Arg.Is(command.ReviewUserId),
				Arg.Is(command.ReviewUserName),
				Arg.Any<string>(),
				Arg.Any<decimal?>());

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task Can_Submit_Withdraw_From_Third_Party_Order_No()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var sut = new SaveWithdrawThirdPartyOrderNoCommandService(fakePaymentRepository);

		var command = new SaveWithdrawThirdPartyOrderNoCommand(Guid.NewGuid(), "thirdPartyOrderNo");

		_ = fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Is(command.FormId))
			.Returns(new WithdrawFormInfo(
				command.FormId,
				"UserId",
				UserType.Player,
				"UserName",
				Guid.NewGuid(),
				1000M,
				null,
				PaymentFlowState.WaitPayment,
				null,
				null,
				null,
				"GPIW211028160001",
				Guid.NewGuid(),
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				null,
				null,
				null,
				HandlingFee: 0M));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SubmitWithdrawFormStatusAsync(
				Arg.Is(command.FormId),
				Arg.Is(PaymentFlowState.WaitPayment),
				Arg.Any<string>(),
				Arg.Any<decimal?>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task Can_Submit_WithdrawForm_Status()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new SubmitWithdrawFormStatusCommandService(
			fakePaymentRepository,
			fakePaymentClient);

		var command = new SubmitWithdrawFormStatusCommand(
			Guid.NewGuid(),
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Is(command.FormId))
			.Returns(new WithdrawFormInfo(
				command.FormId,
				"PlayerCode",
				UserType.Player,
				"UserName",
				Guid.NewGuid(),
				1000M,
				null,
				(int)PaymentFlowState.Init,
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
				null,
				null));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SubmitWithdrawFormStatusAsync(
				Arg.Is(command.FormId),
				Arg.Is(command.Status),
				Arg.Any<string>(),
				Arg.Any<decimal?>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<string>());

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

		var optionsAccessor = new OptionsWrapper<PaymentOptions>(new PaymentOptions());

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			optionsAccessor,
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

		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.ExecuteAsync(command));
	}

	[Test]
	public void Create_Player_Withdraw_Form_If_BankInfo_Not_Self_Will_Fail()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var optionsAccessor = new OptionsWrapper<PaymentOptions>(new PaymentOptions());

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			optionsAccessor,
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

		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.ExecuteAsync(command));
	}

	[Test]
	public void Create_Withdraw_Form_With_Invalid_UserType_Will_Fail()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var optionsAccessor = new OptionsWrapper<PaymentOptions>(new PaymentOptions());

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			optionsAccessor,
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

		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.ExecuteAsync(command));
	}

	[Test]
	public async Task Create_Withdraw_From_BankInfo_Can_Not_Be_Null()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var optionsAccessor = new OptionsWrapper<PaymentOptions>(new PaymentOptions());

		var sut = new CreateWithdrawFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			optionsAccessor,
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
				command.UserId,
				command.BrandId,
				Guid.NewGuid(),
				"Bank",
				JsonDocument.Parse("{}"),
				false));

		fakePaymentRepository
			.When(fake => fake.CreateWithdrawFormAsync(
				Arg.Any<Guid>(),
				Arg.Any<WithdrawFormCreateInfo>()))
			.Do(callInfo =>
			{
				var createInfo = callInfo.Arg<WithdrawFormCreateInfo>();

				Assert.IsNotNull(createInfo.BankInfo);
			});
		await sut.ExecuteAsync(command);
	}
}
