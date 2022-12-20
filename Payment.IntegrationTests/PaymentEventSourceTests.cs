using System.Text.Json;
using Json.Patch;
using Microsoft.Extensions.DependencyInjection;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

public class PaymentEventSourceTests
{
	[Test]
	public async Task DepositForm_EventSource_Apply後應該和快照的資料一樣()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

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

		var formId = Guid.NewGuid();
		var brandPaymentId = Guid.NewGuid();

		_ = await sut.CreateDepositFormAsync(
			formId,
			new Tgs.Payment.Clients.DepositFormCreateInfo
			{
				Amount = 1000M,
				ApplyInfo = "{}",
				BrandId = brandId,
				PlayerCode = "TestPlayer",
				BrandPaymentId = brandPaymentId
			});

		var reviewerId = Guid.NewGuid().ToString();
		var reviewerName = "Reviewer";

		await sut.ReviewDepositFormAsync(
			formId,
			Tgs.Payment.Clients.ReviewState.Approve,
			reviewerId,
			reviewerName,
			"abcde");

		await sut.SubmitDepositFormStatusAsync(
			formId,
			null,
			"{}",
			950M,
			"Test1234",
			null,
			50M);

		var allChanges = dbContext.DepositFormEventSources
			.Where(evt => evt.FormId == formId)
			.OrderBy(evt => evt.Id)
			.Select(evt => JsonSerializer.Deserialize<JsonPatch>(
				evt.ChangeSets,
				default(JsonSerializerOptions))!);

		var actual = default(DepositFormSnapshot);
		foreach (var patch in allChanges)
			actual = patch.Apply(actual, PaymentUtility.SerializerOptions);

		var currentSnapshot = await web.Services
			.GetRequiredService<IPaymentRepository>()
			.GetCurrentDepositFormSnapshotAsync(formId);

		Assert.AreEqual(currentSnapshot, actual);
	}

	[Test]
	public async Task WithdrawForm_EventSource_Apply後應該和快照的資料一樣()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var brandId = Guid.NewGuid();
		var bankInfoId = Guid.NewGuid();

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

		_ = await dbContext.PlayerBankInfos.AddAsync(
			new Tgs.Payment.Repositories.Entities.PlayerBankInfo
			{
				Id = bankInfoId,
				BrandId = brandId,
				BankType = "Bank",
				Detail = "{}",
				PlayerCode = "TestPlayer",
				CreatedByUsername = "TestPlayer",
				UpdatedByUsername = "TestPlayer"
			});

		_ = await dbContext.SaveChangesAsync();

		var brandPaymentId = Guid.NewGuid();

		var formId = await sut.CreateWithdrawFormAsync(
			new Tgs.Payment.Clients.WithdrawFormCreateInfo
			{
				Amount = 1000M,
				BrandId = brandId,
				BankInfoId = bankInfoId,
				Note = "Test",
				UserId = "TestPlayer",
				UserName = "Tester",
				UserType = Tgs.Payment.Clients.UserType.Player
			});

		var reviewerId = Guid.NewGuid().ToString();
		var reviewerName = "Reviewer";

		await sut.ReviewWithdrawFormAsync(
			formId,
			brandPaymentId,
			Tgs.Payment.Clients.ReviewState.Approve,
			reviewerId,
			reviewerName,
			"abcde");

		await sut.SubmitWithdrawFormStatusAsync(
			formId,
			null,
			"{}",
			950M,
			"Test1234",
			null,
			null);

		var allChanges = dbContext.WithdrawFormEventSources
			.Where(evt => evt.FormId == formId)
			.OrderBy(evt => evt.Id)
			.Select(evt => JsonSerializer.Deserialize<JsonPatch>(
				evt.ChangeSets,
				default(JsonSerializerOptions))!);

		var actual = default(WithdrawFormSnapshot);
		foreach (var patch in allChanges)
			actual = patch.Apply(actual, PaymentUtility.SerializerOptions);

		var currentSnapshot = await web.Services
			.GetRequiredService<IPaymentRepository>()
			.GetCurrentWithdrawFormSnapshotAsync(formId);

		Assert.AreEqual(currentSnapshot, actual);
	}

	[Test]
	public async Task WithdrawForm_With_Agent_EventSource_Apply後應該和快照的資料一樣()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var brandId = Guid.NewGuid();
		var bankInfoId = Guid.NewGuid();
		var agentId = Guid.NewGuid();

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

		_ = await dbContext.AgentBankInfoItems.AddAsync(
			new AgentBankInfoItem
			{
				Id = bankInfoId,
				BrandId = brandId,
				BankType = "Bank",
				Detail = "{}",
				AgentId = agentId,
				CreatedByUsername = "TestPlayer",
				UpdatedByUsername = "TestPlayer"
			});

		_ = await dbContext.SaveChangesAsync();

		var brandPaymentId = Guid.NewGuid();

		var formId = await sut.CreateWithdrawFormAsync(
			new Tgs.Payment.Clients.WithdrawFormCreateInfo
			{
				Amount = 1000M,
				BrandId = brandId,
				BankInfoId = bankInfoId,
				Note = "Test",
				UserId = agentId.ToString(),
				UserName = "Tester",
				UserType = Tgs.Payment.Clients.UserType.Agent
			});

		var reviewerId = Guid.NewGuid().ToString();
		var reviewerName = "Reviewer";

		await sut.ReviewWithdrawFormAsync(
			formId,
			brandPaymentId,
			Tgs.Payment.Clients.ReviewState.Approve,
			reviewerId,
			reviewerName,
			"abcde");

		await sut.SubmitWithdrawFormStatusAsync(
			formId,
			null,
			"{}",
			950M,
			"Test1234",
			null,
			null);

		var allChanges = dbContext.WithdrawFormEventSources
			.Where(evt => evt.FormId == formId)
			.OrderBy(evt => evt.Id)
			.Select(evt => JsonSerializer.Deserialize<JsonPatch>(
				evt.ChangeSets,
				default(JsonSerializerOptions))!);

		var actual = default(WithdrawFormSnapshot);
		foreach (var patch in allChanges)
			actual = patch.Apply(actual, PaymentUtility.SerializerOptions);

		var currentSnapshot = await web.Services
			.GetRequiredService<IPaymentRepository>()
			.GetCurrentWithdrawFormSnapshotAsync(formId);

		Assert.AreEqual(currentSnapshot, actual);
	}

	[Test]
	public async Task WithdrawForm_With_Reject_EventSource_Apply後應該和快照的資料一樣()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var brandId = Guid.NewGuid();
		var bankInfoId = Guid.NewGuid();
		var agentId = Guid.NewGuid();

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

		_ = await dbContext.AgentBankInfoItems.AddAsync(
			new AgentBankInfoItem
			{
				Id = bankInfoId,
				BrandId = brandId,
				BankType = "Bank",
				Detail = "{}",
				AgentId = agentId,
				CreatedByUsername = "TestPlayer",
				UpdatedByUsername = "TestPlayer"
			});

		_ = await dbContext.SaveChangesAsync();

		var brandPaymentId = Guid.NewGuid();

		var formId = await sut.CreateWithdrawFormAsync(
			new Tgs.Payment.Clients.WithdrawFormCreateInfo
			{
				Amount = 1000M,
				BrandId = brandId,
				BankInfoId = bankInfoId,
				Note = "Test",
				UserId = agentId.ToString(),
				UserName = "Tester",
				UserType = Tgs.Payment.Clients.UserType.Agent
			});

		var reviewerId = Guid.NewGuid().ToString();
		var reviewerName = "Reviewer";

		await sut.ReviewWithdrawFormAsync(
			formId,
			brandPaymentId,
			Tgs.Payment.Clients.ReviewState.Reject,
			reviewerId,
			reviewerName,
			"abcde");

		await sut.SubmitWithdrawFormStatusAsync(
			formId,
			null,
			"{}",
			950M,
			"Test1234",
			null,
			null);

		var allChanges = dbContext.WithdrawFormEventSources
			.Where(evt => evt.FormId == formId)
			.OrderBy(evt => evt.Id)
			.Select(evt => JsonSerializer.Deserialize<JsonPatch>(
				evt.ChangeSets,
				default(JsonSerializerOptions))!);

		var actual = default(WithdrawFormSnapshot);
		foreach (var patch in allChanges)
			actual = patch.Apply(actual, PaymentUtility.SerializerOptions);

		var currentSnapshot = await web.Services
			.GetRequiredService<IPaymentRepository>()
			.GetCurrentWithdrawFormSnapshotAsync(formId);

		Assert.AreEqual(currentSnapshot, actual);
	}
}
