using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Paging;

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
				DateTimeOffset.Now));

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
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeTakeNumberStore = Substitute.For<ITakeNumberStore>();

		var sut = new CreateDepositFormCommandService(fakePaymentRepository, fakePaymentClient, fakeTakeNumberStore);

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

		_ = fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
			.Returns(new BrandInfo("GPI"));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.GetBrandInfoByIdAsync(Arg.Is(brandId));

		_ = fakePaymentRepository.Received(1)
			.CreateDepositFormAsync(Arg.Is(command.FormId), Arg.Any<DepositFormCreateInfo>());

		_ = fakePaymentRepository.Received(1)
			.SubmitDepositFormStatusAsync(
				Arg.Is(command.FormId),
				Arg.Is(PaymentFlowState.WaitPayment),
				Arg.Any<string>(),
				Arg.Any<decimal?>(),
				Arg.Any<string>(),
				Arg.Any<int?>());

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
				DateTimeOffset.Now));

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
				DateTimeOffset.Now));

		var actual = await sut.GetAsync(query);

		Assert.IsNotNull(actual);
	}

	[Test]
	public async Task Can_Not_SignOff_Form_When_Form_Status_Is_Approved()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new SignOffDepositFormCommandService(fakePaymentRepository, fakePaymentClient);
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
			DateTimeOffset.Now);
		_ = fakePaymentRepository.GetDepositFormByIdAsync(formId).Returns(depositFormInfo);

		var command = new SignOffDepositFormCommand(
			formId,
			ReviewState.Approve,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository
			.Received(0)
			.SignOffDepositFormAsync(
				formId,
				ReviewState.Approve,
				"93a723b2-9ebb-4607-ee8d-08d978fba75f",
				"reviewedUserName");
	}

	[Test]
	public async Task Can_Not_SignOff_Form_When_Form_Status_Is_Reject()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new SignOffDepositFormCommandService(fakePaymentRepository, fakePaymentClient);
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
			DateTimeOffset.Now);

		_ = fakePaymentRepository.GetDepositFormByIdAsync(formId).Returns(depositFormInfo);

		var command = new SignOffDepositFormCommand(
			formId,
			ReviewState.Reject,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository
			.Received(0)
			.SignOffDepositFormAsync(
				formId,
				ReviewState.Approve,
				"93a723b2-9ebb-4607-ee8d-08d978fba75f",
				"reviewedUserName");
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
			new[] {
					PaymentFlowState.WaitPayment,
					PaymentFlowState.WaitApprove,
					PaymentFlowState.Approved,
					PaymentFlowState.Reject
			},
			null,
			null,
			new TGS.Paging.Paging<DepositFormInfo>
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

		var sut = new SignOffDepositFormCommandService(fakePaymentRepository, fakePaymentClient);

		var command = new SignOffDepositFormCommand(
			Guid.NewGuid(),
			ReviewState.Approve,
			"93a723b2-9ebb-4607-ee8d-08d978fba75f",
			"reviewedUserName",
			null);

		_ = fakePaymentRepository.GetDepositFormByIdAsync(Arg.Any<Guid>())
			.Returns(new DepositFormInfo(
				command.FormId,
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
				"{}",
				null,
				null,
				"GPIT211028150001",
				"reviewedUserName",
				DateTimeOffset.Now,
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SignOffDepositFormAsync(
				Arg.Is(command.FormId),
				Arg.Is(command.ReviewState),
				Arg.Is(command.ReviewUserId),
				Arg.Is(command.ReviewUserName),
				Arg.Any<string>());

		_ = fakePaymentClient.Received(1)
			.EmitDepositFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}

	[Test]
	public async Task Can_Submit_DepositForm_Third_Party_Order_No()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new SubmitDepositFormThirdPartyOrderNoCommandService(
			fakePaymentRepository);

		var command = new SubmitDepositFormThirdPartyOrderNoCommand(
			Guid.NewGuid(),
			"ThirdPartyOrderNo");

		_ = fakePaymentRepository.GetDepositFormByIdAsync(Arg.Is(command.FormId))
			.Returns(new DepositFormInfo(
				command.FormId,
				"PlayerCode",
				"UserName",
				Guid.NewGuid(),
				Guid.NewGuid(),
				"Channel1",
				Guid.NewGuid(),
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
				DateTimeOffset.Now,
				DateTimeOffset.Now));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SubmitDepositFormStatusAsync(
				Arg.Is(command.FormId),
				Arg.Is(PaymentFlowState.Approved),
				Arg.Any<string>(),
				Arg.Any<decimal?>(),
				Arg.Is(command.ThirdPartyOrderNo),
				Arg.Any<int?>());
	}

	[Test]
	public async Task Can_Submit_DepostForm_Status()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new SubmitDepositFormStatusCommandService(fakePaymentRepository, fakePaymentClient);

		var command = new SubmitDepositFormStatusCommand(
			Guid.NewGuid(),
			PaymentFlowState.WaitApprove,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.GetDepositFormByIdAsync(Arg.Any<Guid>())
			.Returns(new DepositFormInfo(
				command.FormId,
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
				"{}",
				null,
				null,
				"GPIT211028150001",
				null,
				null,
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SubmitDepositFormStatusAsync(
				Arg.Is(command.FormId),
				Arg.Is(command.Status),
				Arg.Any<string>(),
				Arg.Any<decimal?>(),
				Arg.Any<string>(),
				Arg.Any<int?>());

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
				DateTimeOffset.Now));

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
				DateTimeOffset.Now));

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
	public async Task Submit_DepositForm_With_New_Status_Will_Emit_StatusChangedEvent()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new SubmitDepositFormStatusCommandService(fakePaymentRepository, fakePaymentClient);

		var command = new SubmitDepositFormStatusCommand(
			Guid.NewGuid(),
			PaymentFlowState.Approved,
			null,
			null,
			null,
			null);

		_ = fakePaymentRepository.GetDepositFormByIdAsync(Arg.Any<Guid>())
			.Returns(new DepositFormInfo(
				command.FormId,
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
				"{}",
				null,
				null,
				"GPIT211028150001",
				null,
				null,
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SubmitDepositFormStatusAsync(
				Arg.Is(command.FormId),
				Arg.Is(command.Status),
				Arg.Any<string>(),
				Arg.Any<decimal?>(),
				Arg.Any<string>(),
				Arg.Any<int?>());

		_ = fakePaymentClient.Received(1)
			.EmitDepositFormStatusChangedEventAsync(
				Arg.Is(command.FormId),
				Arg.Any<Guid>(),
				Arg.Any<string>());
	}
}
