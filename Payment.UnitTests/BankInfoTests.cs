using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JSLTSharp;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.BankInfos;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

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
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeBankInfoParserFactory = Substitute.For<IBankInfoIndexParserFactory>();
		var fakeServiceCoordinator = Substitute.For<IServiceCoordinator>();

		var sut = new CreatePlayerBankInfoCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeBankInfoParserFactory,
			new JsonTransform(),
			fakeServiceCoordinator);

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
			JsonSerializer.SerializeToDocument(detail),
			false,
			playerCode);

		_ = fakeBankInfoParserFactory.GetBankInfoTransformation(Arg.Is(command.BankType))
			.Returns("{\"BankName\": \"$.bankName\",\r\n\t\t\t\"BankAccount\": \"$.bankAccount\"}");

		var actual = await sut.GetAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateBankInfoAsync(
				Arg.Is<PlayerBankCreateInfo>(info => info.PaymentChannelId == paymentChannelId),
				Arg.Is(playerCode));
		_ = fakePaymentClient.Received(1)
			.EmitPlayerBankInfoAddedEventAsync(
				Arg.Is(command.BrandId),
				Arg.Is(actual));
	}

	[Test]
	public async Task Can_Create_Player_BankInfo_With_Specified_ModifyUser()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeBankInfoParserFactory = Substitute.For<IBankInfoIndexParserFactory>();
		var fakeServiceCoordinator = Substitute.For<IServiceCoordinator>();

		var sut = new CreatePlayerBankInfoCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeBankInfoParserFactory,
			new JsonTransform(),
			fakeServiceCoordinator);

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
			JsonSerializer.SerializeToDocument(detail),
			false,
			modifyUser);

		_ = fakeBankInfoParserFactory.GetBankInfoTransformation(Arg.Is(command.BankType))
			.Returns("{}");

		var actual = await sut.GetAsync(command);

		_ = fakePaymentRepository.Received(1)
			.CreateBankInfoAsync(
				Arg.Is<PlayerBankCreateInfo>(info => info.PaymentChannelId == paymentChannelId),
				Arg.Is(modifyUser));
		_ = fakePaymentClient.Received(1)
			.EmitPlayerBankInfoAddedEventAsync(
				Arg.Is(command.BrandId),
				Arg.Is(actual));
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
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		var sut = new DeletePlayerBankInfoCommandService(fakePaymentRepository, fakePaymentClient);

		var command = new DeletePlayerBankInfoCommand("ThisIsPlayerCode", Guid.NewGuid());

		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(command.BankInfoId))
			.Returns(new PlayerBankInfo(
				command.BankInfoId,
				"tester1",
				Guid.NewGuid(),
				Guid.NewGuid(),
				"Bank",
				JsonDocument.Parse("{}"),
				false));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.DeleteBankInfoAsync(
				Arg.Is(command.PlayerCode),
				Arg.Is(command.BankInfoId));
		_ = fakePaymentClient.Received(1)
			.EmitPlayerBankInfoDeletedEventAsync(
				Arg.Any<Guid>(),
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

	[Test]
	[Ignore("暫時取消這個功能")]
	public void Create_Player_BankInfo_If_Exists_Will_Throw_Exception()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();
		var fakeBankInfoParserFactory = Substitute.For<IBankInfoIndexParserFactory>();
		var fakeServiceCoordinator = Substitute.For<IServiceCoordinator>();

		var sut = new CreatePlayerBankInfoCommandService(
			fakePaymentRepository,
			fakePaymentClient,
			fakeBankInfoParserFactory,
			new JsonTransform(),
			fakeServiceCoordinator);

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
			JsonSerializer.SerializeToDocument(detail),
			false,
			playerCode);

		_ = fakeBankInfoParserFactory.GetBankInfoTransformation(Arg.Is(command.BankType))
			.Returns("{\"BankName\": \"$.bankName\",\r\n\t\t\t\"BankAccount\": \"$.bankAccount\"}");

		_ = fakePaymentRepository
			.CheckPlayerBankInfoIsExistsAsync(Arg.Is(command.BrandId), Arg.Is(command.BankType), Arg.Any<byte[]>())
			.Returns(true);

		_ = Assert.ThrowsAsync<PlayerBankInfoDuplicateException>(async () => await sut.GetAsync(command));
	}

	[Test]
	public async Task GetPlayers_By_BankInfoTag()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetPlayersByBankInfoTagQueryService(fakePaymentRepository);

		var query = new GetPlayersByBankInfoTagQuery(Guid.NewGuid(), "Test", "123");

		_ = await sut.QueryAsync(query).ToArrayAsync();

		_ = fakePaymentRepository.Received(1)
			.GetPlayerCodesByBankInfoTagAsync(
				Arg.Is(query.BrandId),
				Arg.Is(query.TagType),
				Arg.Is(query.Tag));
	}

	[Test]
	public async Task Process_Player_BankInfo_Index()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeBankInfoIndexParserFactory = Substitute.For<IBankInfoIndexParserFactory>();

		var sut = new ProcessBankInfoTagsCommandService(
			fakePaymentRepository,
			fakeBankInfoIndexParserFactory,
			new JsonTransform(),
			NullLogger<ProcessBankInfoTagsCommandService>.Instance);

		var command = new ProcessBankInfoTagsCommand(Guid.NewGuid());

		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(command.BankInfoId))
			.Returns(new PlayerBankInfo(
				command.BankInfoId,
				"player1",
				Guid.NewGuid(),
				Guid.NewGuid(),
				"Bank",
				JsonSerializer.SerializeToDocument(new
				{
					name = "tester",
					bankName = "bank1",
					bankAccount = "1111111"
				}),
				false));

		_ = fakeBankInfoIndexParserFactory.GetBankInfoTransformation(Arg.Any<string>())
			.Returns(JsonSerializer.Serialize(new
			{
				BankAccount = "$.bankAccount"
			}));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.RemoveBankInfoTagsAsync(Arg.Any<Guid>(), Arg.Is(command.BankInfoId));
		_ = fakePaymentRepository.Received(1)
			.SetBankInfoTagAsync(
				Arg.Any<Guid>(),
				Arg.Is(command.BankInfoId),
				Arg.Is("BankAccount"),
				Arg.Is("player1"),
				Arg.Is("1111111"));
	}

	[Test]
	public async Task Rebuild_PlayerBankInfo_Hash()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();
		var fakeBankInfoIndexParserFactory = Substitute.For<IBankInfoIndexParserFactory>();

		var sut = new RebuildPlayerBankInfoHashCommandService(
			fakePaymentRepository,
			fakeBankInfoIndexParserFactory,
			new JsonTransform());

		var command = new RebuildPlayerBankInfoHashCommand(Guid.NewGuid());

		_ = fakePaymentRepository.GetBankInfoByIdAsync(Arg.Is(command.BankInfoId))
			.Returns(new PlayerBankInfo(
				command.BankInfoId,
				"tester1",
				Guid.NewGuid(),
				Guid.NewGuid(),
				"Bank",
				JsonDocument.Parse("{}"),
				false));

		_ = fakeBankInfoIndexParserFactory.GetBankInfoTransformation(Arg.Any<string>())
			.Returns(JsonSerializer.Serialize(new
			{
				BankAccount = "$.bankAccount"
			}));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.UpdatePlayerBankInfoHashAsync(
				Arg.Is(command.BankInfoId),
				Arg.Any<byte[]>());
	}
}
