using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.BrandCertificate.Clients;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests.Steps;

[Binding]
public class CreateDepositFormSteps
{
	private readonly PaymentServiceClient _paymentServiceClient;
	private readonly TestPaymentApplication _testHost;
	private Guid _createdFormId;
	private DepositFormCreateInfo _depositForm;

	public CreateDepositFormSteps()
	{
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Code = "GPI"
			});

		_testHost = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeBrandServiceClient));

		_paymentServiceClient = _testHost.GetPaymentServiceClient();
	}

	[AfterScenario]
	public void CleanUp() => _testHost.DisposeAsync();

	[Given(@"I will create deposit form")]
	public async Task GivenIWillCreateDepositFormAsync()
	{
		var dbContext = _testHost.Services.GetRequiredService<TgsContext>();

		var brandId = Guid.NewGuid();

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

		_depositForm = new DepositFormCreateInfo
		{
			Amount = 1000M,
			ApplyInfo = "{}",
			BrandId = brandId,
			BrandPaymentId = Guid.NewGuid(),
			PlayerCode = "PlayerCode",
			PromotionId = null
		};
	}

	[Then(@"deposit form created")]
	public void ThenDepositFormCreated()
	{
		Assert.AreNotEqual(Guid.Empty, _createdFormId);
	}

	[When(@"call payment api")]
	public async Task WhenCallPaymentApiAsync()
	{
		_createdFormId = Guid.NewGuid();
		_ = await _paymentServiceClient.CreateDepositFormAsync(_createdFormId, _depositForm);
	}
}
