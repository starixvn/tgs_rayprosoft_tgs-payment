using System.Text.Json;
using Json.Patch;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.Paging;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Payment.UnitTests;

[TestFixture]
public class DepositTests
{
	[Test]
	public async Task BrandId_Not_Match_Can_Not_Get_DepositForm()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetDepositFormDetailQueryService(fakePaymentRepository);

		var query = new GetDepositFormDetailQuery(Guid.NewGuid(), Guid.NewGuid());

		_ = fakePaymentRepository.GetDepositFormByIdAsync(Arg.Any<Guid>())
			.Returns(new DepositFormInfo(
				query.FormId,
				"PlayerCode",
				"UserName",
				Guid.NewGuid(),
				Guid.NewGuid(),
				"ChannelName",
				Guid.NewGuid(),
				1000M,
				null,
				null,
				PaymentFlowState.WaitApprove,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now,
				0M));

		var actual = await sut.GetAsync(query);

		Assert.IsNull(actual);
	}

	[Test]
	public async Task Can_Count_DepositForms()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new DepositFormQueryService(fakePaymentRepository);

		var query = new DepositFormQuery(
			Guid.NewGuid(),
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
			null,
			null,
			null);

		_ = await sut.GetAsync(query);

		_ = fakePaymentRepository.Received(1)
			.CountDepositFormsAsync(Arg.Any<DepositFormQueryConditions>());
	}

	[Test]
	public async Task Can_Create_Deposit_Form()
	{
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();

		var sut = new CreateDepositFormCommandService(
			fakeSerialNumberGenerator,
			fakePaymentRepository,
			fakePaymentClient,
			fakeTakeNumberStore,
			fakeSystemClock);

		var brandId = Guid.NewGuid();
		var command = new CreateDepositFormCommand(
			Guid.NewGuid(),
			new DepositFormCreateInfo
			{
				PlayerCode = "PlayerCode",
				BrandId = brandId,
				BrandPaymentId = Guid.NewGuid(),
				Amount = 1000M,
				PromotionId = null,
				ApplyInfo = null
			});
		var eventId = Guid.NewGuid();
		var orderNo = "Test00001";

		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		_ = fakeTakeNumberStore.GetDepositFormOrderNoAsync(Arg.Is("GPI"))
			.Returns(orderNo);

		var currentDate = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentDate);

		var expectedSnapshot = new DepositFormSnapshot(
			command.FormId,
			command.CreateInfo.PlayerCode,
			command.CreateInfo.BrandId,
			command.CreateInfo.BrandPaymentId,
			command.CreateInfo.Amount,
			null,
			null,
			PaymentFlowState.WaitPayment,
			command.CreateInfo.ApplyInfo,
			null,
			null,
			orderNo,
			null,
			null,
			null,
			command.CreateInfo.PromotionId,
			null,
			currentDate.UtcDateTime,
			0M);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.GetBrandInfoByIdAsync(Arg.Is(brandId));

		_ = fakePaymentRepository.Received(1)
			.CreateDepositFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.Create),
				Arg.Any<JsonDocument>(),
				Arg.Any<string>());

		_ = fakePaymentRepository.Received(1)
			.UpdateDepositFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(expectedSnapshot.CreatedOn));

		_ = fakePaymentClient.Received(1)
			.EmitDepositFormCreatedEventAsync(Arg.Is(command.FormId), Arg.Any<Guid>());
	}

	[Test]
	public async Task Can_Get_One_DepositForm()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetDepositFormDetailQueryService(fakePaymentRepository);

		var query = new GetDepositFormDetailQuery(Guid.NewGuid(), Guid.NewGuid());

		_ = fakePaymentRepository.GetDepositFormByIdAsync(Arg.Is(query.FormId))
			.Returns(new DepositFormInfo(
				query.FormId,
				"PlayerCode",
				"UserName",
				query.BrandId,
				Guid.NewGuid(),
				"ChannelName",
				Guid.NewGuid(),
				1000M,
				null,
				null,
				PaymentFlowState.WaitApprove,
				null,
				null,
				null,
				"GPIT211028150001",
				null,
				null,
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now,
				0M));

		var actual = await sut.GetAsync(query);

		Assert.IsNotNull(actual);
	}

	[Test]
	public async Task Can_Get_One_DepositForm_By_FormId()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetDepositFormDetailByFormIdQueryService(fakePaymentRepository);

		var query = new GetDepositFormDetailByFormIdQuery(Guid.NewGuid());

		_ = fakePaymentRepository.GetDepositFormByIdAsync(Arg.Is(query.FormId))
			.Returns(new DepositFormInfo(
				query.FormId,
				"PlayerCode",
				"UserName",
				Guid.NewGuid(),
				Guid.NewGuid(),
				"ChannelName",
				Guid.NewGuid(),
				1000M,
				null,
				null,
				PaymentFlowState.WaitApprove,
				null,
				null,
				null,
				"GPIT211028150001",
				null,
				null,
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now,
				0M));

		var actual = await sut.GetAsync(query);

		Assert.IsNotNull(actual);
	}

	[Test]
	public async Task Can_Not_SignOff_Form_When_Form_Status_Is_Approved()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SignOffDepositFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var formId = Guid.NewGuid();

		var depositFormInfo = new DepositFormInfo(
			formId,
			"playerCode",
			"userName",
			Guid.NewGuid(),
			Guid.NewGuid(),
			"channelName",
			Guid.NewGuid(),
			100m,
			null,
			null,
			PaymentFlowState.Approved,
			null,
			null,
			null,
			"GPIT211028150001",
			"reviewedUserName",
			DateTimeOffset.Now,
			null,
			null,
			DateTimeOffset.Now,
			DateTimeOffset.Now,
			0M);
		_ = fakePaymentRepository.GetDepositFormByIdAsync(formId).Returns(depositFormInfo);

		var command = new SignOffDepositFormCommand(
			formId,
			ReviewState.Approve,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		await sut.ExecuteAsync(command);

		_ = fakePaymentClient.Received(0)
			.EmitDepositFormStatusChangedEventAsync(
				Arg.Is(formId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task Can_Not_SignOff_Form_When_Form_Status_Is_Reject()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SignOffDepositFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var formId = Guid.NewGuid();

		var depositFormInfo = new DepositFormInfo(
			formId,
			"playerCode",
			"userName",
			Guid.NewGuid(),
			Guid.NewGuid(),
			"channelName",
			Guid.NewGuid(),
			100m,
			null,
			null,
			PaymentFlowState.Reject,
			null,
			null,
			null,
			"GPIT211028150001",
			"reviewedUserName",
			DateTimeOffset.Now,
			null,
			null,
			DateTimeOffset.Now,
			DateTimeOffset.Now,
			0M);

		_ = fakePaymentRepository.GetDepositFormByIdAsync(formId).Returns(depositFormInfo);

		var command = new SignOffDepositFormCommand(
			formId,
			ReviewState.Reject,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		await sut.ExecuteAsync(command);

		_ = fakePaymentClient.Received(0)
			.EmitDepositFormStatusChangedEventAsync(
				Arg.Is(formId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public void Can_Query_DepositForms()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new DepositFormQueryService(fakePaymentRepository);

		var query = new DepositFormQuery(
			Guid.NewGuid(),
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
			null,
			null,
			new Tgs.Paging.Paging<DepositFormInfo>
			{
				Page = -1,
				RowCount = -1
			});

		_ = sut.QueryAsync(query);

		_ = fakePaymentRepository.Received(1)
			.QueryDepositFormsAsync(
				Arg.Any<DepositFormQueryConditions>(),
				Arg.Any<Paging<DepositFormInfo>>());
	}

	[Test]
	public async Task Can_SignOff_Form()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SignOffDepositFormCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SignOffDepositFormCommand(
			Guid.NewGuid(),
			ReviewState.Approve,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		var depositForm = new DepositFormSnapshot(
			command.FormId,
			"PlayerCode",
			Guid.NewGuid(),
			Guid.NewGuid(),
			1000M,
			null,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			"GPIT211028150001",
			null,
			null,
			null,
			null,
			null,
			DateTime.UtcNow,
			0M);
		_ = fakePaymentRepository.GetCurrentDepositFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(depositForm);

		var eventId = Guid.NewGuid();

		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = new DepositFormSnapshot(
			command.FormId,
			"PlayerCode",
			depositForm.BrandId,
			depositForm.BrandPaymentId,
			1000M,
			null,
			null,
			PaymentFlowState.Approved,
			null,
			null,
			null,
			"GPIT211028150001",
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			currentTime.UtcDateTime,
			null,
			null,
			depositForm.CreatedOn,
			0M);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateDepositFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.Review),
				Arg.Any<JsonDocument>(),
				Arg.Is(command.ReviewUserId));

		_ = fakePaymentRepository.Received(1)
			.UpdateDepositFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitDepositFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task Can_Submit_DepostForm_Status()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SubmitDepositFormStatusCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SubmitDepositFormStatusCommand(
			Guid.NewGuid(),
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			null,
			10m);

		var depositForm = new DepositFormSnapshot(
			command.FormId,
			"PlayerCode",
			Guid.NewGuid(),
			Guid.NewGuid(),
			1000M,
			null,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			"GPIT211028150001",
			null,
			null,
			null,
			null,
			null,
			DateTime.UtcNow,
			0M);
		_ = fakePaymentRepository.GetCurrentDepositFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(depositForm);

		var eventId = Guid.NewGuid();

		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = new DepositFormSnapshot(
			command.FormId,
			"PlayerCode",
			depositForm.BrandId,
			depositForm.BrandPaymentId,
			1000M,
			null,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			"GPIT211028150001",
			null,
			null,
			null,
			null,
			null,
			depositForm.CreatedOn,
			10M);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateDepositFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.ChangeStatus),
				Arg.Any<JsonDocument>(),
				Arg.Is(default(string)));

		_ = fakePaymentRepository.Received(1)
			.UpdateDepositFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(0)
			.EmitDepositFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public void Can_Sum_DepositForms()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CalculateDepositTotalAmountService(fakePaymentRepository);

		var query = new DepositFormQuery(
			Guid.NewGuid(),
			null,
			null,
			null,
			ThirdPartyOrderNo: null,
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
			null,
			null,
			null);

		_ = sut.GetAsync(query);

		_ = fakePaymentRepository.Received(1)
			.CalculateDepositTotalAmountAsync(Arg.Any<DepositFormQueryConditions>());
	}

	[Test]
	public async Task LockPayment_But_Has_Previous_Form()
	{
		var fakePaymentRateLimitRepository = Substitute.For<IPaymentRateLimitRepository>();
		var fakeGetFormService = Substitute.For<IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo>>();

		var sut = new LockPaymentCommandService(
			fakePaymentRateLimitRepository,
			fakeGetFormService);

		var command = new LockPaymentCommand(
			Guid.NewGuid(),
			"TestPayment",
			"TestPlayerCode");

		var previousFormId = Guid.NewGuid();

		_ = fakePaymentRateLimitRepository
			.LockPaymentAsync(
				Arg.Is(command.FormId),
				Arg.Is(command.PaymentCode),
				Arg.Is(command.PlayerCode))
			.Returns(previousFormId);

		_ = fakeGetFormService.GetAsync(Arg.Any<GetDepositFormDetailByFormIdQuery>())
			.Returns(new DepositFormInfo(
				previousFormId,
				command.PlayerCode,
				"Tester",
				Guid.NewGuid(),
				Guid.NewGuid(),
				"AAA",
				Guid.NewGuid(),
				100M,
				null,
				null,
				PaymentFlowState.WaitApprove,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now,
				0M));

		var actual = await sut.GetAsync(command);

		Assert.AreEqual(previousFormId, actual);
	}

	[Test]
	public async Task LockPayment_Has_Previous_Id_But_Form_Completed()
	{
		var fakePaymentRateLimitRepository = Substitute.For<IPaymentRateLimitRepository>();
		var fakeGetFormService = Substitute.For<IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo>>();

		var sut = new LockPaymentCommandService(
			fakePaymentRateLimitRepository,
			fakeGetFormService);

		var command = new LockPaymentCommand(
			Guid.NewGuid(),
			"TestPayment",
			"TestPlayerCode");

		var previousFormId = Guid.NewGuid();

		_ = fakePaymentRateLimitRepository
			.LockPaymentAsync(
				Arg.Is(command.FormId),
				Arg.Is(command.PaymentCode),
				Arg.Is(command.PlayerCode))
			.Returns(previousFormId);

		_ = fakeGetFormService
			.GetAsync(Arg.Any<GetDepositFormDetailByFormIdQuery>())
			.Returns(new DepositFormInfo(
				previousFormId,
				command.PlayerCode,
				"Tester",
				Guid.NewGuid(),
				Guid.NewGuid(),
				"AAA",
				Guid.NewGuid(),
				100M,
				null,
				null,
				PaymentFlowState.Approved,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now,
				0M));

		fakePaymentRateLimitRepository
			.When(fake => fake.UnlockPaymentAsync(
				Arg.Is(command.PaymentCode),
				Arg.Is(command.PlayerCode)))
			.Do(_ => fakePaymentRateLimitRepository
				.LockPaymentAsync(
					Arg.Is(command.FormId),
					Arg.Is(command.PaymentCode),
					Arg.Is(command.PlayerCode))
				.Returns(command.FormId));

		var actual = await sut.GetAsync(command);

		Assert.AreEqual(command.FormId, actual);
	}

	[Test]
	public async Task LockPayment_Has_Previous_Id_Form_Creating()
	{
		var fakePaymentRateLimitRepository = Substitute.For<IPaymentRateLimitRepository>();
		var fakeGetFormService = Substitute.For<IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo>>();

		var sut = new LockPaymentCommandService(
			fakePaymentRateLimitRepository,
			fakeGetFormService);

		var command = new LockPaymentCommand(
			Guid.NewGuid(),
			"TestPayment",
			"TestPlayerCode");

		var previousFormId = Guid.NewGuid();

		_ = fakePaymentRateLimitRepository
			.LockPaymentAsync(
				Arg.Is(command.FormId),
				Arg.Is(command.PaymentCode),
				Arg.Is(command.PlayerCode))
			.Returns(previousFormId);

		var actual = await sut.GetAsync(command);

		Assert.AreEqual(previousFormId, actual);
	}

	[Test]
	public async Task LockPayment_When_Deposit()
	{
		var fakePaymentRateLimitRepository = Substitute.For<IPaymentRateLimitRepository>();
		var fakeGetFormService = Substitute.For<IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo>>();

		var sut = new LockPaymentCommandService(
			fakePaymentRateLimitRepository,
			fakeGetFormService);

		var command = new LockPaymentCommand(
			Guid.NewGuid(),
			"TestPayment",
			"TestPlayerCode");

		_ = fakePaymentRateLimitRepository
			.LockPaymentAsync(
				Arg.Is(command.FormId),
				Arg.Is(command.PaymentCode),
				Arg.Is(command.PlayerCode))
			.Returns(command.FormId);

		var actual = await sut.GetAsync(command);

		Assert.AreEqual(command.FormId, actual);
	}

	[Test]
	public void PaymentReturn變更記錄應該要記錄正確內容()
	{
		var depositForm = new DepositFormSnapshot(
			Guid.NewGuid(),
			"PlayerCode",
			Guid.NewGuid(),
			Guid.NewGuid(),
			1000M,
			null,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			"GPIT211028150001",
			null,
			null,
			null,
			null,
			null,
			DateTime.UtcNow,
			0M);

		var sut = depositForm with
		{
			PaymentReturn = "{\"manualReceiptFileId\": \"639fffd50c20c141ae1992bc\"}"
		};

		var patch = depositForm.CreatePatch(sut, PaymentUtility.SerializerOptions);

		var actual = patch.Apply(depositForm, PaymentUtility.SerializerOptions);

		Assert.AreEqual(sut, actual);
	}

	[Test]
	public async Task Submit_DepositForm_With_New_Status_Will_Emit_StatusChangedEvent()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeSystemClock = Substitute.For<Tgs.Payment.ISystemClock>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new SubmitDepositFormStatusCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeSystemClock,
			fakeSerialNumberGenerator);

		var command = new SubmitDepositFormStatusCommand(
			Guid.NewGuid(),
			PaymentFlowState.Approved,
			null,
			null,
			null,
			null,
			10m);

		var depositForm = new DepositFormSnapshot(
			command.FormId,
			"PlayerCode",
			Guid.NewGuid(),
			Guid.NewGuid(),
			1000M,
			null,
			null,
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			"GPIT211028150001",
			null,
			null,
			null,
			null,
			null,
			DateTime.UtcNow,
			0M);
		_ = fakePaymentRepository.GetCurrentDepositFormSnapshotAsync(Arg.Is(command.FormId))
			.Returns(depositForm);

		var eventId = Guid.NewGuid();

		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var currentTime = DateTimeOffset.UtcNow;
		_ = fakeSystemClock.UtcNow.Returns(currentTime);

		var expectedSnapshot = new DepositFormSnapshot(
			command.FormId,
			"PlayerCode",
			depositForm.BrandId,
			depositForm.BrandPaymentId,
			1000M,
			null,
			null,
			PaymentFlowState.Approved,
			null,
			null,
			null,
			"GPIT211028150001",
			null,
			null,
			null,
			null,
			null,
			depositForm.CreatedOn,
			10M);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateDepositFormEventAsync(
				Arg.Is(eventId),
				Arg.Is(command.FormId),
				Arg.Is(EventAction.ChangeStatus),
				Arg.Any<JsonDocument>(),
				Arg.Is(default(string)));

		_ = fakePaymentRepository.Received(1)
			.UpdateDepositFormSnapshotAsync(
				Arg.Is(expectedSnapshot),
				Arg.Is(eventId),
				Arg.Is(currentTime.UtcDateTime));

		_ = fakePaymentClient.Received(1)
			.EmitDepositFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task UnlockPayment()
	{
		var fakePaymentRateLimitRepository = Substitute.For<IPaymentRateLimitRepository>();

		var sut = new UnlockPaymentCommandService(fakePaymentRateLimitRepository);

		var command = new UnlockPaymentCommand(
			"TestPayment",
			"player1");

		await sut.ExecuteAsync(command);

		_ = fakePaymentRateLimitRepository.Received(1)
			.UnlockPaymentAsync(Arg.Is(command.PaymentCode), Arg.Is(command.PlayerCode));
	}

	[Test]
	public async Task 存款成功率全部單為0時()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CurrentDaySuccessRateQueryService(fakePaymentRepository);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

		_ = fakePaymentRepository
			.CountCurrentDayDepositFormsStatusAsync(Arg.Is(brandId))
			.Returns(new DepositFormsStatusCount { Approved = 0, Total = 0 });

		var actual = await sut.GetAsync(new CurrentDaySuccessRateQuery(brandId));

		Assert.AreEqual(0m, actual);
	}

	[Test]
	public async Task 存款成功率整數四捨五入()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CurrentDaySuccessRateQueryService(fakePaymentRepository);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

		_ = fakePaymentRepository
			.CountCurrentDayDepositFormsStatusAsync(Arg.Is(brandId))
			.Returns(new DepositFormsStatusCount { Approved = 2, Total = 3 });

		var actual = await sut.GetAsync(new CurrentDaySuccessRateQuery(brandId));

		Assert.AreEqual(67m, actual);
	}

	[Test]
	public async Task 存款成功率整數四捨五入2()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CurrentDaySuccessRateQueryService(fakePaymentRepository);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

		_ = fakePaymentRepository
			.CountCurrentDayDepositFormsStatusAsync(Arg.Is(brandId))
			.Returns(new DepositFormsStatusCount { Approved = 1, Total = 3 });

		var actual = await sut.GetAsync(new CurrentDaySuccessRateQuery(brandId));

		Assert.AreEqual(33m, actual);
	}

	[Test]
	public async Task 站台存提差_加總已通過存款金額()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeCountRepository = Substitute.For<IBrandNetDepositRepository>();

		var sut = new CountDepositApprovedCommandService(fakePaymentRepository, fakeCountRepository, NullLogger<CountDepositApprovedCommandService>.Instance);

		var brandId = Guid.NewGuid();
		var formId = Guid.NewGuid();
		var amount = 168m;

		var depositForm = new DepositFormInfo(
			formId,
			"playerCode",
			"userName",
			brandId,
			Guid.NewGuid(),
			"channelName",
			Guid.NewGuid(),
			amount,
			null,
			null,
			PaymentFlowState.WaitPayment,
			null,
			null,
			null,
			null,
			"reviewedUsername",
			DateTimeOffset.Now,
			null,
			null,
			DateTimeOffset.Now,
			DateTimeOffset.Now,
			0m);

		_ = fakePaymentRepository
			.GetDepositFormByIdAsync(formId)
			.Returns(depositForm);

		await sut.ExecuteAsync(new CountDepositApprovedCommand(brandId, formId, PaymentFlowState.WaitApprove));

		_ = fakeCountRepository
			.Received(1)
			.CountBrandNetDepositAsync(Arg.Is(brandId), Arg.Is((double)amount));
	}

	[Test]
	public async Task 站台存提差_只處理已通過存款單()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeCountRepository = Substitute.For<IBrandNetDepositRepository>();

		var sut = new CountDepositApprovedCommandService(fakePaymentRepository, fakeCountRepository, NullLogger<CountDepositApprovedCommandService>.Instance);

		await sut.ExecuteAsync(new CountDepositApprovedCommand(Guid.NewGuid(), Guid.NewGuid(), PaymentFlowState.Approved));

		_ = fakePaymentRepository
			.Received(0)
			.GetDepositFormByIdAsync(Arg.Any<Guid>());

		_ = fakeCountRepository
			.Received(0)
			.CountBrandNetDepositAsync(Arg.Any<Guid>(), Arg.Any<double>());
	}
}
