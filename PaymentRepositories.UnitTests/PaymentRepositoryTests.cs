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

		var bankInfos = await dbContext.PlayerBankInfos
			.Where(x => x.PlayerCode == playerCode)
			.ToArrayAsync();

		var actual1 = bankInfos.FirstOrDefault(x => x.Id == defaultBankInfo.Id);

		Assert.IsNotNull(actual1);
		Assert.IsFalse(actual1.Default);

		var actual2 = bankInfos.FirstOrDefault(x => x.Id == newDefaultBankInfo.Id);

		Assert.IsNotNull(actual2);
		Assert.IsTrue(actual2.Default);
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
	public async Task Submit_DepositForm_Status_Without_PaymentReturn_The_Data_Will_Keep_Original()
	{
		var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
		_ = dbContextOptionsBuilder
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.ConfigureWarnings(x => x.Ignore(
				InMemoryEventId.TransactionIgnoredWarning));

		var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

		var testFormData = new DepositForm
		{
			Id = Guid.NewGuid(),
			PlayerCode = "Tester",
			Status = 0,
			PaymentReturn = "Test"
		};

		_ = await dbContext.DepositForms.AddAsync(testFormData);

		_ = await dbContext.SaveChangesAsync();

		var sut = new PaymentRepository(dbContext);

		await sut.SubmitDepositFormStatusAsync(
			testFormData.Id,
			Tgs.Payment.PaymentFlowState.Paid,
			null,
			null,
			null,
			null);

		Assert.AreEqual("Test", testFormData.PaymentReturn);
	}
}
