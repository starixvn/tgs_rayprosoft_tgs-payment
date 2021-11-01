using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Clients;

namespace Payment.UnitTests
{
	[TestFixture]
	public class DepositTests
	{
		[Test]
		public async Task BrandId_Not_Match_Can_Not_Get_DepositForm()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new GetDepositFormDetailQueryService(fakePaymentRepository);

			var query = new GetDepositFormDetailQuery(Guid.NewGuid(), Guid.NewGuid());

			fakePaymentRepository.GetDepositFormByIdAsync(Arg.Any<Guid>())
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
					  PaymentFlowState.WaitApprove,
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
		public void Can_Count_DepositForms()
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

			var actual = sut.GetAsync(query);

			fakePaymentRepository.Received(1)
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

			fakePaymentRepository.GetBrandInfoByIdAsync(brandId)
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
					Arg.Any<string>());

			_ = fakePaymentClient.Received(1)
				.EmitDepositFormCreatedEventAsync(Arg.Is(command.FormId), Arg.Any<Guid>());
		}

		[Test]
		public async Task Can_Get_One_DepositForm()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new GetDepositFormDetailQueryService(fakePaymentRepository);

			var query = new GetDepositFormDetailQuery(Guid.NewGuid(), Guid.NewGuid());

			fakePaymentRepository.GetDepositFormByIdAsync(Arg.Is(query.FormId))
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
					  PaymentFlowState.WaitApprove,
					  null,
					  null,
					  null,
					  "GPIT211028150001",
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

			fakePaymentRepository.GetDepositFormByIdAsync(Arg.Is(query.FormId))
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
					  PaymentFlowState.WaitApprove,
					  null,
					  null,
					  null,
					  "GPIT211028150001",
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
				PaymentFlowState.Approved,
				null,
				null,
				null,
				"GPIT211028150001",
				"reviewedUserName",
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now);
			fakePaymentRepository.GetDepositFormByIdAsync(formId).Returns(depositFormInfo);

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
				PaymentFlowState.Reject,
				null,
				null,
				null,
				"GPIT211028150001",
				"reviewedUserName",
				null,
				null,
				DateTimeOffset.Now,
				DateTimeOffset.Now);
			fakePaymentRepository.GetDepositFormByIdAsync(formId).Returns(depositFormInfo);

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
				null);

			var actual = sut.QueryAsync(query);

			fakePaymentRepository.Received(1)
				.QueryDepositFormsAsync(Arg.Any<DepositFormQueryConditions>());
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

			fakePaymentRepository.GetDepositFormByIdAsync(Arg.Any<Guid>())
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
					PaymentFlowState.WaitApprove,
					"{}",
					null,
					null,
					"GPIT211028150001",
					"reviewedUserName",
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
				.EmitDepositFormStatusChangedEventAsync(Arg.Is(command.FormId), Arg.Any<Guid>());
		}

		[Test]
		public async Task Can_Submit_DepositForm_Third_Party_Order_No()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();
			var fakePaymentClient = Substitute.For<IPaymentClient>();

			var sut = new SubmitDepositFormThirdPartyOrderNoCommandService(
				fakePaymentRepository,
				fakePaymentClient);

			var command = new SubmitDepositFormThirdPartyOrderNoCommand(
				Guid.NewGuid(),
				"ThirdPartyOrderNo");

			fakePaymentRepository.GetDepositFormByIdAsync(Arg.Is(command.FormId))
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
					PaymentFlowState.Approved,
					null,
					null,
					null,
					"GPIT211028150001",
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
					Arg.Is(command.ThirdPartyOrderNo));
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
				null);

			fakePaymentRepository.GetDepositFormByIdAsync(Arg.Any<Guid>())
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
					PaymentFlowState.WaitApprove,
					"{}",
					null,
					null,
					"GPIT211028150001",
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
					Arg.Any<string>());

			_ = fakePaymentClient.Received(0)
				.EmitDepositFormStatusChangedEventAsync(Arg.Is(command.FormId), Arg.Any<Guid>());
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

			var actual = sut.GetAsync(query);

			fakePaymentRepository.Received(1)
				.CalculateDepositTotalAmountAsync(Arg.Any<DepositFormQueryConditions>());
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
				null);

			fakePaymentRepository.GetDepositFormByIdAsync(Arg.Any<Guid>())
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
					PaymentFlowState.WaitApprove,
					"{}",
					null,
					null,
					"GPIT211028150001",
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
					Arg.Any<string>());

			_ = fakePaymentClient.Received(1)
				.EmitDepositFormStatusChangedEventAsync(Arg.Is(command.FormId), Arg.Any<Guid>());
		}
	}
}
