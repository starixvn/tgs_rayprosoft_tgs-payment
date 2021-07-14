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

			var sut = new CreateDepositFormCommandService(fakePaymentRepository, fakePaymentClient);

			var command = new CreateDepositFormCommand(
				Guid.NewGuid(),
				new DepositFormCreateInfo(
					"PlayerCode",
					Guid.NewGuid(),
					Guid.NewGuid(),
					1000M,
					null,
					null));

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.CreateDepositFormAsync(Arg.Is(command.FormId), Arg.Any<DepositFormCreateInfo>());

			_ = fakePaymentRepository.Received(1)
				.SubmitDepositFormStatusAsync(
				Arg.Is(command.FormId),
				PaymentFlowState.WaitPayment,
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

			fakePaymentRepository.GetDepositFormByIdAsync(Arg.Any<Guid>())
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
					  DateTimeOffset.Now,
					  DateTimeOffset.Now));

			var actual = await sut.GetAsync(query);

			Assert.IsNotNull(actual);
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
					DateTimeOffset.Now,
					DateTimeOffset.Now));

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.SignOffDepositFormAsync(
					Arg.Is(command.FormId),
					Arg.Is(command.ReviewState),
					Arg.Any<string>());

			_ = fakePaymentClient.Received(1)
				.EmitDepositFormStatusChangedEventAsync(Arg.Is(command.FormId), Arg.Any<Guid>());
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
					DateTimeOffset.Now,
					DateTimeOffset.Now));

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.SubmitDepositFormStatusAsync(
					Arg.Is(command.FormId),
					Arg.Is(command.Status),
					Arg.Any<string>());

			_ = fakePaymentClient.Received(1)
				.EmitDepositFormStatusChangedEventAsync(Arg.Is(command.FormId), Arg.Any<Guid>());
		}
	}
}
