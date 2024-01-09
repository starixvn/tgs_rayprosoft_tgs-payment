using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Legacy;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using Tgs.PlayerService.Client;

namespace Payment.IntegrationTests;

public class WithdrawTests
{
	[Test]
	public async Task 拒絕提款單後_表單應該為Reject_並且會將申請金額Rollback回錢包()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeCashServiceClient)
			.AddSingleton(fakePlayerServiceClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var formId = Guid.NewGuid();
		var playerCode = "test1";
		var playerWalletId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.WithdrawForms.AddAsync(new WithdrawForm
		{
			Id = formId,
			BrandId = Guid.NewGuid(),
			UserId = playerCode,
			UserName = "Player",
			UserType = (int)UserType.Player,
			Amount = 1000M,
			Status = (int)PaymentFlowState.WaitApprove,
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now,
			BankInfo = "{}"
		});

		_ = await dbContext.SaveChangesAsync();

		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = playerWalletId.ToString()
			}.ToImmutableDictionary());

		await sut.ReviewWithdrawFormAsync(
			formId,
			null,
			ReviewState.Reject,
			null,
			null,
			null);
		var actual = await dbContext.WithdrawForms.FindAsync(formId);

		ClassicAssert.NotNull(actual);
		ClassicAssert.AreEqual((int)PaymentFlowState.Reject, actual.Status);

		_ = fakeCashServiceClient.Received(1)
			.RollbackWithdrawAsync(
				Arg.Is(playerWalletId),
				Arg.Is(1000M),
				Arg.Any<string>(),
				Arg.Is(formId.ToString()),
				Arg.Any<Guid?>(),
				Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task 拒絕代理提款單後_表單應該為Reject_並且會將申請金額Rollback回錢包()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakeAgentClient = Substitute.For<IAgentClient>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeCashServiceClient)
			.AddSingleton(fakeAgentClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var formId = Guid.NewGuid();
		var agentId = Guid.NewGuid();
		var agentWalletId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.WithdrawForms.AddAsync(new WithdrawForm
		{
			Id = formId,
			BrandId = Guid.NewGuid(),
			UserId = agentId.ToString(),
			UserName = "Agent",
			UserType = (int)UserType.Agent,
			Amount = 1000M,
			Status = (int)PaymentFlowState.WaitApprove,
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now,
			BankInfo = "{}"
		});

		_ = await dbContext.SaveChangesAsync();

		_ = fakeAgentClient.GetAgentInfoAsync(Arg.Is(new GetAgentInfoRequest(agentId)), Arg.Any<CancellationToken>())
			.Returns(new AgentInfoModel(
				agentId,
				"Agent",
				null,
				null,
				null,
				null,
				null,
				"aa1234",
				null,
				DateTimeOffset.UtcNow,
				AgentStatus.Enabled,
				AgentType.Cash,
				AgentLevelType.Agent,
				0M,
				0,
				0M,
				Guid.NewGuid(),
				Guid.NewGuid(),
				agentWalletId,
				false));

		await sut.ReviewWithdrawFormAsync(
			formId,
			null,
			ReviewState.Reject,
			null,
			null,
			null);
		var actual = await dbContext.WithdrawForms.FindAsync(formId);

		ClassicAssert.NotNull(actual);
		ClassicAssert.AreEqual((int)PaymentFlowState.Reject, actual.Status);

		_ = fakeCashServiceClient.Received(1)
			.RollbackWithdrawAsync(
				Arg.Is(agentWalletId),
				Arg.Is(1000M),
				Arg.Any<string>(),
				Arg.Is(formId.ToString()),
				Arg.Any<Guid?>(),
				Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task 完成提款單後_表單應該為Paid_並且會Commit申請金額()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeCashServiceClient)
			.AddSingleton(fakePlayerServiceClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var formId = Guid.NewGuid();
		var playerCode = "test1";
		var playerWalletId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.WithdrawForms.AddAsync(new WithdrawForm
		{
			Id = formId,
			BrandId = Guid.NewGuid(),
			UserId = playerCode,
			UserName = "Player",
			UserType = (int)UserType.Player,
			Amount = 1000M,
			Status = (int)PaymentFlowState.Approved,
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now,
			BankInfo = "{}"
		});

		_ = await dbContext.SaveChangesAsync();

		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = playerWalletId.ToString()
			}.ToImmutableDictionary());

		await sut.ReviewWithdrawFormAsync(
			formId,
			null,
			ReviewState.Paid,
			null,
			null,
			null);
		var actual = await dbContext.WithdrawForms.FindAsync(formId);

		ClassicAssert.NotNull(actual);
		ClassicAssert.AreEqual((int)PaymentFlowState.Paid, actual.Status);

		_ = fakeCashServiceClient.Received(1)
			.CommitWithdrawAsync(
				Arg.Is(playerWalletId),
				Arg.Is(1000M),
				Arg.Is(1000M),
				Arg.Any<string>(),
				Arg.Is(formId.ToString()),
				Arg.Any<Guid?>(),
				Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task 完成代理提款單後_表單應該為Paid_並且會Commit申請金額()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakeAgentClient = Substitute.For<IAgentClient>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeCashServiceClient)
			.AddSingleton(fakeAgentClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var formId = Guid.NewGuid();
		var agentId = Guid.NewGuid();
		var agentWalletId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.WithdrawForms.AddAsync(new WithdrawForm
		{
			Id = formId,
			BrandId = Guid.NewGuid(),
			UserId = agentId.ToString(),
			UserName = "Agent",
			UserType = (int)UserType.Agent,
			Amount = 1000M,
			Status = (int)PaymentFlowState.Approved,
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now,
			BankInfo = "{}"
		});

		_ = await dbContext.SaveChangesAsync();

		_ = fakeAgentClient.GetAgentInfoAsync(Arg.Is(new GetAgentInfoRequest(agentId)), Arg.Any<CancellationToken>())
			.Returns(new AgentInfoModel(
				agentId,
				"Agent",
				null,
				null,
				null,
				null,
				null,
				"aa1234",
				null,
				DateTimeOffset.UtcNow,
				AgentStatus.Enabled,
				AgentType.Cash,
				AgentLevelType.Agent,
				0M,
				0,
				0M,
				Guid.NewGuid(),
				Guid.NewGuid(),
				agentWalletId,
				false));

		await sut.ReviewWithdrawFormAsync(
			formId,
			null,
			ReviewState.Paid,
			null,
			null,
			null);
		var actual = await dbContext.WithdrawForms.FindAsync(formId);

		ClassicAssert.NotNull(actual);
		ClassicAssert.AreEqual((int)PaymentFlowState.Paid, actual.Status);

		_ = fakeCashServiceClient.Received(1)
			.CommitWithdrawAsync(
				Arg.Is(agentWalletId),
				Arg.Is(1000M),
				Arg.Is(1000M),
				Arg.Any<string>(),
				Arg.Is(formId.ToString()),
				Arg.Any<Guid?>(),
				Arg.Any<CancellationToken>());
	}
}
