using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework.Legacy;
using TechTalk.SpecFlow;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using Tgs.BrandCertificate.Clients;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.VipLevelService.Clients;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Create agent withdraw form")]
public class CreateAgentWithdrawFormSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _agentId;
	private Guid _bankInfoId;
	private Guid _createFormId;

	public CreateAgentWithdrawFormSteps()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakeAgentClient = Substitute.For<IAgentClient>();

		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Code = "GPI"
			});

		var walletId = Guid.NewGuid();
		_ = fakeAgentClient.GetAgentInfoAsync(
			Arg.Any<GetAgentInfoRequest>(),
			Arg.Any<CancellationToken>())
			.Returns(new AgentInfoModel(
				Guid.NewGuid(),
				"test1",
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
				walletId,
				false));

		_ = fakeCashServiceClient.GetBalanceAsync(
			Arg.Is(walletId),
			Arg.Any<CancellationToken>())
			.Returns(10000M);
		_ = fakeCashServiceClient.LockWithdrawAsync(
			Arg.Is(walletId),
			Arg.Any<decimal>(),
			Arg.Any<string>(),
			Arg.Any<string>(),
			Arg.Any<CancellationToken>())
			.Returns(new TransactionResponse
			{
				Balance = 1000M,
				LockedBalance = 1000M,
				TransactionId = Guid.NewGuid()
			});

		_testHost = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeCashServiceClient)
			.AddSingleton(fakeAgentClient)
			.AddSingleton(fakeBrandServiceClient));

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"agentId is \{(.*)}")]
	public void GivenAgentIdIs(Guid agentId)
	{
		_agentId = agentId;
		_bankInfoId = Guid.NewGuid();
	}

	[Given(@"I will create withdraw form")]
	public void GivenIWillCreateWithdrawForm()
	{
	}

	[Then(@"withdraw form created")]
	public void ThenWithdrawFormCreated()
	{
		ClassicAssert.AreNotEqual(Guid.Empty, _createFormId);
	}

	[When(@"call create withdraw form api")]
	public async Task WhenCallCreateWithdrawFormApiAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var brandId = Guid.NewGuid();

		_ = await dbContext.AgentBankInfoItems.AddAsync(
			new AgentBankInfoItem
			{
				Id = _bankInfoId,
				BankType = "Bank",
				BrandId = brandId,
				AgentId = _agentId,
				Detail = "{}",
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now,
				CreatedByUsername = "System",
				UpdatedByUsername = "System",
			});

		_ = await dbContext.Brands.AddAsync(
			new Brand
			{
				Id = brandId,
				Code = "GPI",
				ApiKey = "apikey",
				ClientId = "clientId",
				CreatedByUsername = "System",
				Name = "test",
				SecretKey = "secret",
				UpdatedByUsername = "System"
			});

		_ = await dbContext.SaveChangesAsync();

		var withdrawForm = new WithdrawFormCreateInfo
		{
			Amount = 1000M,
			BankInfoId = _bankInfoId,
			UserId = _agentId.ToString(),
			UserName = "AgentName",
			BrandId = brandId,
			UserType = UserType.Agent
		};

		_createFormId = await _paymentServiceClient.CreateWithdrawFormAsync(withdrawForm);
	}
}
