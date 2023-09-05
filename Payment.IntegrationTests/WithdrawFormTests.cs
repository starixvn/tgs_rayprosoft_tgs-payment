using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

public class WithdrawFormTests
{
	[Test]
	public async Task 建立WithdrawForm會發出Event()
	{
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePaymentClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var bankInfoId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.PlayerBankInfos.AddAsync(
			new Tgs.Payment.Repositories.Entities.PlayerBankInfo
			{
				Id = bankInfoId,
				BankType = "Bank",
				BrandId = brandId,
				PlayerCode = playerCode,
				Detail = "{}",
				CreatedOn = DateTime.Now,
				UpdatedOn = DateTime.Now,
				CreatedByUsername = "System",
				UpdatedByUsername = "System"
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

		var withdrawForm = new Tgs.Payment.Clients.WithdrawFormCreateInfo
		{
			Amount = 1000M,
			BankInfoId = bankInfoId,
			UserId = playerCode,
			UserName = "UserName",
			BrandId = brandId,
			UserType = Tgs.Payment.Clients.UserType.Player
		};

		var actual = await sut.CreateWithdrawFormAsync(withdrawForm);

		_ = fakePaymentClient.Received(1)
			.EmitWithdrawFormCreatedEventAsync(
				Arg.Is(actual),
				Arg.Is(brandId),
				Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task 查詢WithdrawForms給FixTool使用()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakePaymentRepository));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var userType = Tgs.Payment.Clients.UserType.Player;

		_ = await sut.QueryWithdrawFormsAsync(
			brandId,
			playerCode,
			userType).ToArrayAsync();

		_ = fakePaymentRepository.Received(1)
			.QueryWithdrawFormsAsync(
				Arg.Is(Tgs.Payment.WithdrawFormQueryConditions.Default with
				{
					BrandId = brandId,
					UserId = playerCode,
					UserType = Tgs.Payment.UserType.Player
				}),
				Arg.Is(false),
				Arg.Any<CancellationToken>());
	}
}
