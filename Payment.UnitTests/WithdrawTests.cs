using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices;
using Tgs.Payment.Clients;

namespace Payment.UnitTests
{
	[TestFixture]
	public class WithdrawTests
	{
		[Test]
		public async Task Can_Create_Withdraw_Form()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();
			var fakePaymentClient = Substitute.For<IPaymentClient>();

			var sut = new CreateWithdrawFormCommandService(
				fakePaymentRepository,
				fakePaymentClient);

			var command = new CreateWithdrawFormCommand(
				"ThisIsPlayerCode",
				"TestUser",
				UserType.Player,
				Guid.NewGuid(),
				Guid.NewGuid(),
				1000M);

			fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(command.BankInfoId))
				.Returns(new PlayerBankInfo(
					command.BankInfoId,
					command.UserId,
					command.BrandId,
					Guid.NewGuid(),
					"Bank",
					"{}"));

			await sut.ExecuteAsync(command);

			var actual = await command.ReturnFormIdSource.Task;

			Assert.AreNotEqual(Guid.Empty, actual);

			_ = fakePaymentRepository.Received(1)
				.CreateWithdrawFormAsync(Arg.Is(actual), Arg.Any<WithdrawFormCreateInfo>());

			_ = fakePaymentClient.Received(1)
				.EmitWithdrawFormCreatedEventAsync(Arg.Is(actual), Arg.Any<Guid>());
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
				null);

			fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Is(command.FormId))
				.Returns(new WithdrawFormInfo(
					command.FormId,
					"PlayerCode",
					UserType.Player,
					"UserName",
					Guid.NewGuid(),
					1000M,
					(int)PaymentFlowState.Init,
					null,
					Guid.NewGuid(),
					DateTimeOffset.Now,
					DateTimeOffset.Now,
					null,
					null,
					null,
					null,
					null,
					null));

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.SubmitWithdrawFormStatusAsync(
					Arg.Is(command.FormId),
					Arg.Is(command.Status),
					Arg.Any<string>(),
					Arg.Any<decimal?>());

			_ = fakePaymentClient.Received(1)
				.EmitWithdrawFormStatusChangedEventAsync(
					Arg.Is(command.FormId),
					Arg.Any<Guid>());
		}
	}
}
