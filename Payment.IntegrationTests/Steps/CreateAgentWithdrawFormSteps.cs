using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using Tgs.Payment.WithdrawLimit;
using Tgs.Payment.WithdrawLimit.Clients;
using Tgs.PlayerService.Client;
using Tgs.VipLevelService.Clients;
using PlayerInfo = Tgs.PlayerService.Client.PlayerInfo;

namespace Payment.IntegrationTests.Steps;

[Binding]
[Scope(Scenario = "Create agent withdraw form")]
public class CreateAgentWithdrawFormSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private Guid _agentId;
	private Guid _bankInfoId;
	private Guid _createFormId;
	private readonly TestPaymentApplication _testHost;

	public CreateAgentWithdrawFormSteps()
	{
		var fakeVipLevelClient = Substitute.For<IVipLevelServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

		_testHost = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(Substitute.For<IVipLevelServiceClient>())
			.AddSingleton(Substitute.For<IPlayerServiceClient>()));

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
		Assert.AreNotEqual(Guid.Empty, _createFormId);
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
