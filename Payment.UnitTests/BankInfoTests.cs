using System;
using System.Text.Json;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices;

namespace Payment.UnitTests
{
	[TestFixture]
	public class BankInfoTests
	{
		[Test]
		public async Task Can_Create_Player_BankInfo()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new CreatePlayerBankInfoCommandService(fakePaymentRepository);

			var brandId = Guid.NewGuid();
			var playerCode = "ThisIsPlayerCode";
			var paymentChannelId = Guid.NewGuid();
			var bankType = "Bank";
			var detail = new
			{
				bankAccount = "1234567890"
			};

			var command = new CreatePlayerBankInfoCommand(
				brandId,
				playerCode,
				paymentChannelId,
				bankType,
				JsonSerializer.Serialize(detail));

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.CreateBankInfoAsync(
					Arg.Is<PlayerBankInfo>(info => info.PaymentChannelId == paymentChannelId),
					Arg.Is(playerCode));
		}

		[Test]
		public async Task Can_Delete_Player_BankInfo()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new DeletePlayerBankInfoCommandService(fakePaymentRepository);

			var command = new DeletePlayerBankInfoCommand("ThisIsPlayerCode", Guid.NewGuid());

			await sut.ExecuteAsync(command);

			_ = fakePaymentRepository.Received(1)
				.DeleteBankInfoAsync(
					Arg.Is(command.PlayerCode),
					Arg.Is(command.BankInfoId));
		}

		[Test]
		public void Can_List_BankInfos_By_PlayerCode()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new GetPlayerBankInfosQueryService(fakePaymentRepository);

			var query = new GetPlayerBankInfosQuery("ThisIsPlayerCode");

			sut.QueryAsync(query);

			fakePaymentRepository.Received(1)
				.GetBankInfosByPlayerCodeAsync(Arg.Is(query.PlayerCode));
		}
	}
}
