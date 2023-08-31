using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using Tgs.Payment.Repositories.Entities;
using TGS.Payment.Repositories;

namespace PaymentRepositories.UnitTests;

public class PaymentRepositoryTests
{
	[Test]
	public async Task Can_Add_Brand_Auto_Withdraw_Event_Source()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var sut = new PaymentRepository(dbContext);

		var eventId = Guid.Parse("018a218f-b4cb-0000-0001-07e7e07dc945");

		await sut.CreateBrandAutoWithdrawEventAsync(
			eventId,
			Guid.NewGuid(),
			JsonDocument.Parse("{}"),
			"50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5");

		var eventSoruce = await dbContext.BrandPaymentAutoWithdrawEventSources.FirstAsync(x => x.Id == eventId);

		Assert.AreEqual(eventId, eventSoruce.Id);
	}

	[Test]
	public async Task Can_Add_Brand_Auto_Withdraw_Setting()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var sut = new PaymentRepository(dbContext);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		var brandPaymentIds = Guid.Parse("ef798189-a8c9-4d34-917c-694a2f8c5a0e");

		await sut.UpdateBrandAutoWithdrawSnapshotAsync(
			new Tgs.Payment.AutoWithdrawInfo(true, 100M, new Guid[] { brandPaymentIds }),
			brandId,
			Guid.NewGuid(),
			DateTime.UtcNow);

		var brandAutoWithdrawInfo = await dbContext.BrandPaymentAutoWithdraws.FirstOrDefaultAsync(b => b.BrandId == brandId);

		Assert.NotNull(brandAutoWithdrawInfo);
		Assert.AreEqual(brandId, brandAutoWithdrawInfo.BrandId);
		Assert.True(brandAutoWithdrawInfo.EnableAutoWithdraw);
		Assert.AreEqual(100M, brandAutoWithdrawInfo.AmountUpperLimit);
		Assert.AreEqual("{\"brandPaymentIds\":[\"ef798189-a8c9-4d34-917c-694a2f8c5a0e\"]}", brandAutoWithdrawInfo.BrandPaymentCandidates);
	}

	[Test]
	public async Task Can_Only_Set_One_Player_BankInfo_To_Default()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandId = Guid.NewGuid();
		var playerCode = "4ucmqplxb029";
		var bankType = "Bank";
		var modifyUser = "bo_admin";

		var defaultBankInfo = new PlayerBankInfo
		{
			Id = Guid.NewGuid(),
			PlayerCode = playerCode,
			BrandId = brandId,
			BankType = bankType,
			PaymentChannelId = Guid.NewGuid(),
			Default = true,
			Detail = JsonSerializer.Serialize(new { bankAccount = "1234567890" }),
			CreatedByUsername = playerCode,
			CreatedOn = DateTime.UtcNow,
			UpdatedByUsername = playerCode,
			UpdatedOn = DateTime.UtcNow
		};

		var newDefaultBankInfo = new PlayerBankInfo
		{
			Id = Guid.NewGuid(),
			PlayerCode = playerCode,
			BrandId = brandId,
			BankType = bankType,
			PaymentChannelId = Guid.NewGuid(),
			Default = false,
			Detail = JsonSerializer.Serialize(new { bankAccount = "0987654321" }),
			CreatedByUsername = playerCode,
			CreatedOn = DateTime.UtcNow,
			UpdatedByUsername = playerCode,
			UpdatedOn = DateTime.UtcNow
		};

		var testBankInfoData = new[] { defaultBankInfo, newDefaultBankInfo };

		await dbContext.PlayerBankInfos.AddRangeAsync(testBankInfoData);

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		await sut.SetDefaultPlayerBankInfoAsync(playerCode, newDefaultBankInfo.Id, modifyUser);

		Assert.IsFalse(defaultBankInfo.Default);
		Assert.IsTrue(newDefaultBankInfo.Default);
	}

	[Test]
	public async Task Can_Only_Set_One_Player_BankInfo_To_Default_With_Two_No_Default_Records()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandId = Guid.NewGuid();
		var playerCode = "4ucmqplxb029";
		var bankType = "Bank";
		var modifyUser = "bo_admin";

		var defaultBankInfo = new PlayerBankInfo
		{
			Id = Guid.NewGuid(),
			PlayerCode = playerCode,
			BrandId = brandId,
			BankType = bankType,
			PaymentChannelId = Guid.NewGuid(),
			Default = true,
			Detail = JsonSerializer.Serialize(new { bankAccount = "1234567890" }),
			CreatedByUsername = playerCode,
			CreatedOn = DateTime.UtcNow,
			UpdatedByUsername = playerCode,
			UpdatedOn = DateTime.UtcNow
		};

		var noneDefaultBankInfo = new PlayerBankInfo
		{
			Id = Guid.NewGuid(),
			PlayerCode = playerCode,
			BrandId = brandId,
			BankType = bankType,
			PaymentChannelId = Guid.NewGuid(),
			Default = false,
			Detail = JsonSerializer.Serialize(new { bankAccount = "8765432109" }),
			CreatedByUsername = playerCode,
			CreatedOn = DateTime.UtcNow,
			UpdatedByUsername = playerCode,
			UpdatedOn = DateTime.UtcNow
		};

		var newDefaultBankInfo = new PlayerBankInfo
		{
			Id = Guid.NewGuid(),
			PlayerCode = playerCode,
			BrandId = brandId,
			BankType = bankType,
			PaymentChannelId = Guid.NewGuid(),
			Default = false,
			Detail = JsonSerializer.Serialize(new { bankAccount = "0987654321" }),
			CreatedByUsername = playerCode,
			CreatedOn = DateTime.UtcNow,
			UpdatedByUsername = playerCode,
			UpdatedOn = DateTime.UtcNow
		};

		var testBankInfoData = new[] { defaultBankInfo, noneDefaultBankInfo, newDefaultBankInfo };

		await dbContext.PlayerBankInfos.AddRangeAsync(testBankInfoData);

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		await sut.SetDefaultPlayerBankInfoAsync(playerCode, newDefaultBankInfo.Id, modifyUser);

		Assert.IsFalse(defaultBankInfo.Default);
		Assert.IsFalse(noneDefaultBankInfo.Default);
		Assert.IsTrue(newDefaultBankInfo.Default);
	}

	[Test]
	public async Task Can_Set_Player_BankInfo_To_Default()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandId = Guid.NewGuid();
		var playerCode = "4ucmqplxb029";
		var bankType = "Bank";
		var modifyUser = "bo_admin";

		var testBankInfoData = new PlayerBankInfo
		{
			Id = Guid.NewGuid(),
			PlayerCode = playerCode,
			BrandId = brandId,
			BankType = bankType,
			PaymentChannelId = Guid.NewGuid(),
			Default = false,
			Detail = JsonSerializer.Serialize(new { bankAccount = "0987654321" }),
			CreatedByUsername = playerCode,
			CreatedOn = DateTime.UtcNow,
			UpdatedByUsername = playerCode,
			UpdatedOn = DateTime.UtcNow
		};

		_ = await dbContext.PlayerBankInfos.AddAsync(testBankInfoData);

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		await sut.SetDefaultPlayerBankInfoAsync(playerCode, testBankInfoData.Id, modifyUser);

		var actual = await dbContext.PlayerBankInfos.FirstOrDefaultAsync(x => x.Id == testBankInfoData.Id);

		Assert.NotNull(actual);
		Assert.IsTrue(actual.Default);
	}

	[Test]
	public async Task Can_Update_Brand_Auto_Withdraw_Setting()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
		_ = await dbContext.BrandPaymentAutoWithdraws.AddAsync(new BrandPaymentAutoWithdraw
		{
			BrandId = brandId,
			EnableAutoWithdraw = true,
			SnapshotEventId = Guid.Parse("018a218f-b4cb-0000-0001-07e7e07dc945"),
			AmountUpperLimit = 100M,
			BrandPaymentCandidates = """
				{
					brandPaymentIds: []
				}
			"""
		});

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		await sut.UpdateBrandAutoWithdrawSnapshotAsync(
			new Tgs.Payment.AutoWithdrawInfo(false, 168M, new Guid[] { Guid.Parse("9a5afdb0-0502-49fe-b87f-0890c77be996") }),
			brandId,
			Guid.Parse("018a2190-10f1-0000-0001-07e7e07dc945"),
			DateTime.UtcNow);

		var brandAutoWithdrawInfo = await dbContext.BrandPaymentAutoWithdraws.FirstOrDefaultAsync(b => b.BrandId == brandId);

		Assert.AreEqual(brandId, brandAutoWithdrawInfo.BrandId);
		Assert.False(brandAutoWithdrawInfo.EnableAutoWithdraw);
		Assert.AreEqual(168M, brandAutoWithdrawInfo.AmountUpperLimit);
		Assert.AreEqual("{\"brandPaymentIds\":[\"9a5afdb0-0502-49fe-b87f-0890c77be996\"]}", brandAutoWithdrawInfo.BrandPaymentCandidates);
	}

	[Test]
	public async Task Count_Current_Day_Deposit_Forms_Status()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

		await dbContext.DepositForms.AddRangeAsync(new DepositForm[]
		{
			new DepositForm
			{
				Id = Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8"),
				BrandId = brandId,
				Status = 8000,
				PlayerCode = "4uel5ke0uf3k",
				CreatedOn = DateTime.UtcNow,
			},
			new DepositForm
			{
				Id = Guid.Parse("072a90e2-8912-4a78-3ccc-08d9262ce1a5"),
				BrandId = brandId,
				Status = 8000,
				PlayerCode = "4uel5ke0uf3k",
				CreatedOn = DateTime.UtcNow,
			},
			new DepositForm
			{
				Id = Guid.Parse("5cd08736-f83c-4134-babf-85671bb1cfeb"),
				BrandId = brandId,
				Status = 7000,
				PlayerCode = "4uel5ke0uf3k",
				CreatedOn = DateTime.UtcNow,
			}
		});

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		var actual = await sut.CountCurrentDayDepositFormsStatusAsync(brandId);

		Assert.AreEqual(1, actual.Reject);
		Assert.AreEqual(2, actual.Approved);
		Assert.AreEqual(3, actual.Total);
	}

	[Test]
	public async Task Get_Brand_Auto_Withdraw_Setting()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandId = Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8");
		var brandPaymentIds = Guid.Parse("2a4c7802-9448-4c80-97ac-c6ac2c8978b8");

		_ = await dbContext.BrandPaymentAutoWithdraws.AddAsync(
			new BrandPaymentAutoWithdraw
			{
				BrandId = brandId,
				AmountUpperLimit = 168M,
				BrandPaymentCandidates = $$"""
					{
						"brandPaymentIds": ["{{brandPaymentIds}}"]
					}
					""",
				EnableAutoWithdraw = true,
				SnapshotEventId = Guid.NewGuid()
			}
		);

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		var actual = await sut.GetBrandAutoWithdrawSettingAsync(brandId);

		Assert.True(actual.EnableAutoWithdraw);
		Assert.AreEqual(168M, actual.AmountUpperLimit);
		Assert.AreEqual(1, actual.BrandPaymentIds.Count());
		Assert.AreEqual(brandPaymentIds, actual.BrandPaymentIds.First());
	}

	[Test]
	public async Task Get_Brand_Auto_Withdraw_Setting2()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandId = Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8");

		var sut = new PaymentRepository(dbContext);

		var actual = await sut.GetBrandAutoWithdrawSettingAsync(brandId);

		Assert.IsNull(actual);
	}

	[Test]
	public async Task Get_BrandPayment_Fee_Settings()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandPaymentId = Guid.NewGuid();

		var brandPayment = new BrandPayment
		{
			Id = brandPaymentId,
			ReducePlayerAmount = true,
			HandlingFeeRateTenPercentile = 10m,
			ChannelName = "This is channel name",
			ExtendedData = "{}",
			CreatedByUsername = "createdUsername",
			UpdatedByUsername = "updatedUsername",
		};

		_ = await dbContext.BrandPayments.AddAsync(brandPayment);

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		var actual = await sut.GetBrandPaymentFeeSettingAsync(brandPaymentId);

		Assert.AreEqual(10m, actual.HandlingFeeRateTenPercentile);
		Assert.AreEqual(true, actual.ReducePlayerAmount);
	}

	[Test]
	public async Task Get_BrandPayments_Fee_Settings()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandPaymentIds = new Guid[]
		{
			Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8"),
			Guid.Parse("072a90e2-8912-4a78-3ccc-08d9262ce1a5")
		};

		var brandPayment = new BrandPayment
		{
			Id = Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8"),
			ReducePlayerAmount = true,
			HandlingFeeRateTenPercentile = 10M,
			ChannelName = "This is channel name",
			ExtendedData = "{}",
			CreatedByUsername = "createdUsername",
			UpdatedByUsername = "updatedUsername",
		};

		await dbContext.BrandPayments.AddRangeAsync(new BrandPayment[]
		{
			new BrandPayment
			{
				Id = Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8"),
				ReducePlayerAmount = true,
				HandlingFeeRateTenPercentile = 10M,
				ChannelName = "This is channel name",
				ExtendedData = "{}",
				CreatedByUsername = "createdUsername",
				UpdatedByUsername = "updatedUsername",
			},
			new BrandPayment
			{
				Id = Guid.Parse("072a90e2-8912-4a78-3ccc-08d9262ce1a5"),
				ReducePlayerAmount = false,
				HandlingFeeRateTenPercentile = 0M,
				ChannelName = "This is channel name2",
				ExtendedData = "{}",
				CreatedByUsername = "createdUsername2",
				UpdatedByUsername = "updatedUsername2",
			}
		});

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		var actual = await sut.GetBrandPaymentFeeSettingsAsync(brandPaymentIds).ToArrayAsync();

		Assert.AreEqual(2, actual.Length);
	}

	[Test]
	public async Task Update_BrandPayment_Extended_Data()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandPaymentId = Guid.NewGuid();

		var brandPayment = new BrandPayment
		{
			Id = brandPaymentId,
			ReducePlayerAmount = true,
			HandlingFeeRateTenPercentile = 10m,
			ChannelName = "This is channel name",
			ExtendedData = "{}",
			CreatedByUsername = "createdUsername",
			UpdatedByUsername = "updatedUsername",
		};

		_ = await dbContext.BrandPayments.AddAsync(brandPayment);

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		var extendedData = JsonDocument.Parse("{\"name\":\"Test\"}");

		await sut.UpdateBrandPaymentExtendedDataAsync(brandPaymentId, extendedData);

		Assert.AreEqual("{\"name\":\"Test\"}", dbContext.BrandPayments.FirstAsync(bp => bp.Id == brandPaymentId).Result.ExtendedData);
	}
}
