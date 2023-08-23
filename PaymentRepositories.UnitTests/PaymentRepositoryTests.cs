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
	public async Task Get_Brand_Auto_Withdraw_Setting()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var brandId = Guid.Parse("6482dba3-e6a2-4b91-a299-638a5fb082a8");

		_ = await dbContext.BrandPaymentAutoWithdraws.AddAsync(
			new BrandPaymentAutoWithdraw
			{
				BrandId = brandId,
				AmountUpperLimit = 168M,
				BrandPaymentCandidates = "{}",
				EnableAutoWithdraw = true,
				SnapshotEventId = Guid.NewGuid()
			}
		);

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		var actual = await sut.GetBrandAutoWithdrawSettingAsync(brandId);

		Assert.True(actual.EnableAutoWithdraw);
		Assert.AreEqual(168M, actual.AmountUpperLimit);
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
