using System;
using System.Text.Json;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.BankInfos;

namespace Payment.UnitTests;

[TestFixture]
public class BankInfoTests
{
	[Test]
	public async Task Can_Create_Agent_BankInfo()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new CreateAgentBankInfoCommandService(fakePaymentRepository);

		var command = new CreateAgentBankInfoCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			"TestAgent",
			Guid.NewGuid(),
			"Bank",
			JsonSerializer.Serialize(new
			{
				bankAccount = "0987654321"
			}));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository
			.Received(1)
			.CreateAgentBankInfoAsync(
				Arg.Is<AgentBankInfo>(info => info.PaymentChannelId == command.PaymentChannelId),
				Arg.Any<string>());
	}

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
			JsonSerializer.Serialize(detail),
			playerCode);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateBankInfoAsync(
				Arg.Is<PlayerBankInfo>(info => info.PaymentChannelId == paymentChannelId),
				Arg.Is(playerCode));
	}

	[Test]
	public async Task Can_Create_Player_BankInfo_With_Specified_ModifyUser()
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

		var modifyUser = "ThisIsModifyUser";

		var command = new CreatePlayerBankInfoCommand(
			brandId,
			playerCode,
			paymentChannelId,
			bankType,
			JsonSerializer.Serialize(detail),
			modifyUser);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateBankInfoAsync(
				Arg.Is<PlayerBankInfo>(info => info.PaymentChannelId == paymentChannelId),
				Arg.Is(modifyUser));
	}

	[Test]
	public async Task Can_Delete_Agent_BankInfo()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new DeleteAgentBankInfoCommandService(fakePaymentRepository);

		var agentId = Guid.NewGuid();
		var bankInfoId = Guid.NewGuid();
		var command = new DeleteAgentBankInfoCommand(agentId, bankInfoId);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository
			.Received(1)
			.DeleteAgentBankInfoAsync(Arg.Is(agentId), Arg.Is(bankInfoId));
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
	public void Can_List_BankInfos_By_AgentId()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetAgentBankInfosQueryService(fakePaymentRepository);

		var agentId = Guid.NewGuid();

		_ = sut.QueryAsync(new GetAgentBankInfosQuery(agentId));

		_ = fakePaymentRepository
			.Received(1)
			.GetAgentBankInfosByAgentIdAsync(Arg.Is(agentId));
	}

	[Test]
	public void Can_List_BankInfos_By_PlayerCode()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetPlayerBankInfosQueryService(fakePaymentRepository);

		var query = new GetPlayerBankInfosQuery("ThisIsPlayerCode");

		_ = sut.QueryAsync(query);

		_ = fakePaymentRepository.Received(1)
			.GetBankInfosByPlayerCodeAsync(Arg.Is(query.PlayerCode));
	}

	[Test]
	public async Task Can_Set_Player_BankInfo_To_Default()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new SetDefaultPlayerBankInfoCommandService(fakePaymentRepository);

		var command = new SetDefaultPlayerBankInfoCommand(
			"ThisIsPlayerCode",
			Guid.NewGuid(),
			"ThisIsModifyUser");

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository
			.Received(1)
			.SetDefaultPlayerBankInfoAsync(
				Arg.Is(command.PlayerCode),
				Arg.Is(command.BankInfoId),
				Arg.Is(command.ModifyUser));
	}
}
