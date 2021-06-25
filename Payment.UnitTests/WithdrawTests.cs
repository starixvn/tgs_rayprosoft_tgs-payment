using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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

			var optionsAccessor = new OptionsWrapper<PaymentOptions>(new PaymentOptions());

			var sut = new CreateWithdrawFormCommandService(
				fakePaymentRepository,
				fakePaymentClient,
				optionsAccessor);

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
		public async Task Can_SignOff_Form()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();
			var fakeWithdrawProcessorFactory = Substitute.For<IWithdrawProcessorFactory>();
			var fakePaymentClient = Substitute.For<IPaymentClient>();

			var sut = new SignOffWithdrawFormCommandService(
				fakePaymentRepository,
				fakeWithdrawProcessorFactory,
				fakePaymentClient);

			var command = new SignOffWithdrawFormCommand(
				Guid.NewGuid(),
				Guid.NewGuid(),
				ReviewState.Approve,
				null);

			fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Any<Guid>())
				.Returns(new WithdrawFormInfo(
					command.FormId,
					"ThisIsPlayerCode",
					UserType.Player,
					"UserName",
					Guid.NewGuid(),
					1000M,
					PaymentFlowState.WaitApprove,
					null,
					Guid.NewGuid(),
					DateTimeOffset.UtcNow,
					DateTimeOffset.UtcNow));

			fakePaymentRepository
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
					true));

			fakePaymentRepository.GetPaymentAsync(Arg.Any<Guid>())
				.Returns(new PaymentInfo(Guid.NewGuid(), "LeePay"));

			fakeWithdrawProcessorFactory.GetWithdrawProcessor(Arg.Any<string>())
				.Returns(Substitute.For<IWithdrawProcessor>());

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.SignOffWithdrawFormAsync(
					Arg.Is(command.FormId),
					Arg.Is(command.BrandPaymentId),
					Arg.Any<string>(),
					Arg.Is(command.Status),
					Arg.Any<string>());

			_ = fakePaymentClient.Received(1)
				.EmitWithdrawFormStatusChangedEventAsync(Arg.Is(command.FormId), Arg.Any<Guid>());
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

		[Test]
		public async Task Create_Withdraw_From_BankInfo_Can_Not_Be_Null()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();
			var fakePaymentClient = Substitute.For<IPaymentClient>();

			var optionsAccessor = new OptionsWrapper<PaymentOptions>(new PaymentOptions());

			var sut = new CreateWithdrawFormCommandService(
				fakePaymentRepository,
				fakePaymentClient,
				optionsAccessor);

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

		[Test]
		public async Task Can_Get_One_WithdrawForm()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new GetWithdrawFormDetailQueryService(fakePaymentRepository);

			var query = new GetWithdrawFormDetailQuery(Guid.NewGuid(), Guid.NewGuid());

			fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Any<Guid>())
				.Returns(new WithdrawFormInfo(
					query.FormId,
					  "PlayerCode",
					  UserType.Player,
					  "UserName",
					  query.BrandId,
					  1000M,
					  PaymentFlowState.WaitApprove,
					  null,
					  Guid.NewGuid(),
					  DateTimeOffset.Now,
					  DateTimeOffset.Now,
					  null,
					  null,
					  null,
					  null,
					  null));

			var actual = await sut.GetAsync(query);

			Assert.IsNotNull(actual);
		}

		[Test]
		public async Task BrandId_Not_Match_Can_Not_Get_WithdrawForm()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new GetWithdrawFormDetailQueryService(fakePaymentRepository);

			var query = new GetWithdrawFormDetailQuery(Guid.NewGuid(), Guid.NewGuid());

			fakePaymentRepository.GetWithdrawFormByIdAsync(Arg.Any<Guid>())
				.Returns(new WithdrawFormInfo(
					query.FormId,
					  "PlayerCode",
					  UserType.Player,
					  "UserName",
					  Guid.NewGuid(),
					  1000M,
					  PaymentFlowState.WaitApprove,
					  null,
					  Guid.NewGuid(),
					  DateTimeOffset.Now,
					  DateTimeOffset.Now,
					  null,
					  null,
					  null,
					  null,
					  null));

			var actual = await sut.GetAsync(query);

			Assert.IsNull(actual);
		}
	}
}
