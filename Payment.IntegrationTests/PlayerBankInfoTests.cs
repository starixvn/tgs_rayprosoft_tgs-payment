using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

public class PlayerBankInfoTests
{
	[Test]
	public async Task Set_PlayerBankInfo_Tags()
	{
		var bankInfoParserOptions = new OptionsWrapper<BankInfoParserOptions>(new BankInfoParserOptions
		{
			["Bank"] = JsonSerializer.SerializeToDocument(new
			{
				BankAccount = "$.bankAccount"
			})
		});
		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton<IOptions<BankInfoParserOptions>>(bankInfoParserOptions));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var bankInfoId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.PlayerBankInfos.AddAsync(new Tgs.Payment.Repositories.Entities.PlayerBankInfo
		{
			Id = bankInfoId,
			BrandId = Guid.NewGuid(),
			BankType = "Bank",
			PlayerCode = "tester1",
			Detail = JsonSerializer.Serialize(new
			{
				bankName = "test bank",
				bankAccount = "111111",
				name = "Tester"
			}),
			CreatedByUsername = "system",
			UpdatedByUsername = "system"
		});

		_ = await dbContext.SaveChangesAsync();

		await sut.SetPlayerBankInfoTagsAsync(bankInfoId);

		var count = dbContext.PlayerBankInfoTags.Count();
		var actual = dbContext.PlayerBankInfoTags.First();

		Assert.AreEqual(1, count);
		Assert.AreEqual("BankAccount", actual.TagType);
		Assert.AreEqual("111111", actual.Tag);
	}
}
