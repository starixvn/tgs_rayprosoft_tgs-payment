using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework.Legacy;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

public class PlayerBankInfoTests
{
	[Test]
	public async Task GetPlayerCodes_By_BankInfoTag()
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

		var brandId = Guid.NewGuid();
		var tagType = "Test";
		var tag = "123";

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		await dbContext.PlayerBankInfoTags.AddRangeAsync(
			new PlayerBankInfoTag
			{
				BrandId = brandId,
				TagType = "Test",
				Tag = "123",
				BankInfoId = Guid.NewGuid(),
				PlayerCode = "player1"
			},
			new PlayerBankInfoTag
			{
				BrandId = brandId,
				TagType = "ABC",
				Tag = "123",
				BankInfoId = Guid.NewGuid(),
				PlayerCode = "player2"
			},
			new PlayerBankInfoTag
			{
				BrandId = brandId,
				TagType = "Test",
				Tag = "456",
				BankInfoId = Guid.NewGuid(),
				PlayerCode = "player3"
			});

		_ = await dbContext.SaveChangesAsync();

		var actual = await sut.GetPlayersByBankInfoTagAsync(brandId, tagType, tag).ToArrayAsync();

		var expected = new[] { "player1" };
		CollectionAssert.AreEquivalent(expected, actual);
	}

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

		ClassicAssert.AreEqual(1, count);
		ClassicAssert.AreEqual("BankAccount", actual.TagType);
		ClassicAssert.AreEqual("111111", actual.Tag);
	}

	[Test]
	public async Task 建立玩家銀行卡_如果Account一樣_Channel不一樣_可以通過()
	{
		var bankInfoParserOptions = new OptionsWrapper<BankInfoParserOptions>(new BankInfoParserOptions
		{
			["Bank"] = JsonSerializer.SerializeToDocument(new
			{
				BankName = "$.bankName",
				BankAccount = "$.bankAccount"
			})
		});
		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton<IOptions<BankInfoParserOptions>>(bankInfoParserOptions));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var bankInfoId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var paymentChannelId = Guid.NewGuid();

		var createInfo1 = new BankCreateInfo
		{
			BankType = "Bank",
			Detail = JsonSerializer.SerializeToDocument(new
			{
				bankName = "test bank",
				bankAccount = "111111",
				name = "Tester"
			}),
			IsDefault = false,
			PaymentChannelId = paymentChannelId
		};

		var createInfo2 = new BankCreateInfo
		{
			BankType = "Bank",
			Detail = JsonSerializer.SerializeToDocument(new
			{
				bankName = "test bank",
				bankAccount = "111111",
				name = "Tester"
			}),
			IsDefault = false,
			PaymentChannelId = Guid.NewGuid()
		};

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.BrandPlayerBankinfoCheckFlags
			.AddAsync(new BrandPlayerBankinfoCheckFlag
			{
				BrandId = brandId,
				IsEnabled = true
			});

		_ = await dbContext.SaveChangesAsync();

		_ = await sut.CreatePlayerBankInfoAsync(brandId, "tester1", createInfo1);

		var actual = await sut.CreatePlayerBankInfoAsync(brandId, "tester2", createInfo2);

		var dbRecord = await dbContext.PlayerBankInfos.FindAsync(actual);

		ClassicAssert.AreEqual(createInfo2.PaymentChannelId, dbRecord.PaymentChannelId);
	}

	[Test]
	public async Task 建立玩家銀行卡如果Channel_Account一樣會出錯()
	{
		var bankInfoParserOptions = new OptionsWrapper<BankInfoParserOptions>(new BankInfoParserOptions
		{
			["Bank"] = JsonSerializer.SerializeToDocument(new
			{
				BankName = "$.bankName",
				BankAccount = "$.bankAccount"
			})
		});
		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton<IOptions<BankInfoParserOptions>>(bankInfoParserOptions));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var bankInfoId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var paymentChannelId = Guid.NewGuid();

		var createInfo = new BankCreateInfo
		{
			BankType = "Bank",
			Detail = JsonSerializer.SerializeToDocument(new
			{
				bankName = "test bank",
				bankAccount = "111111",
				name = "Tester"
			}),
			IsDefault = false,
			PaymentChannelId = paymentChannelId
		};

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.BrandPlayerBankinfoCheckFlags
			.AddAsync(new BrandPlayerBankinfoCheckFlag
			{
				BrandId = brandId,
				IsEnabled = true
			});

		_ = await dbContext.SaveChangesAsync();

		_ = await sut.CreatePlayerBankInfoAsync(brandId, "tester1", createInfo);

		_ = Assert.ThrowsAsync<Tgs.Payment.Clients.PlayerBankInfoDuplicateException>(
			async () => await sut.CreatePlayerBankInfoAsync(brandId, "tester2", createInfo));
	}

	[Test]
	public async Task 建立玩家銀行卡時_玩家已經建滿平台允許的銀行卡數量()
	{
		var bankInfoParserOptions = new OptionsWrapper<BankInfoParserOptions>(new BankInfoParserOptions
		{
			["Bank"] = JsonSerializer.SerializeToDocument(new
			{
				BankName = "$.bankName",
				BankAccount = "$.bankAccount"
			})
		});
		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton<IOptions<BankInfoParserOptions>>(bankInfoParserOptions)
			.AddHttpClientLoggingHandler());

		var httpClient = web.CreateDefaultClient(web.Services.ResolveLoggingHandler<PaymentServiceClient>());

		var sut = new PaymentServiceClient(httpClient);

		var bankInfoId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var paymentChannelId = Guid.NewGuid();

		var createInfo = new BankCreateInfo
		{
			BankType = "Bank",
			Detail = JsonSerializer.SerializeToDocument(new
			{
				bankName = "test bank",
				bankAccount = "111111",
				name = "Tester"
			}),
			IsDefault = false,
			PaymentChannelId = paymentChannelId
		};

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.BrandPaymentSettings.AddAsync(new BrandPaymentSetting
		{
			BrandId = brandId,
			PlayerBankInfoCountLimit = 0
		});

		_ = await dbContext.SaveChangesAsync();

		_ = Assert.ThrowsAsync<Tgs.Payment.Clients.PlayerBankInfoCountReachedLimitException>(
			async () => await sut.CreatePlayerBankInfoAsync(brandId, "tester2", createInfo));
	}
}
