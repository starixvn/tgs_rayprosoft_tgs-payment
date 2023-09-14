using System.Collections.Immutable;
using System.Text.Json;
using Json.Patch;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.CashService.Client;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using Tgs.Payment.WithdrawLimit;
using Tgs.Payment.WithdrawLimit.Clients;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.VipLevelService.Clients;

namespace Payment.IntegrationTests;

public class PaymentEventSourceTests
{
	[Test]
	public async Task DepositForm_EventSource_Apply後應該和快照的資料一樣()
	{
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeBrandServiceClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var brandId = Guid.NewGuid();

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Id = brandId,
				Code = "GPI"
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
		var brandId = Guid.NewGuid();
		var bankInfoId = Guid.NewGuid();

		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
		var fakeVipLevelClient = Substitute.For<IVipLevelServiceClient>();
		var fakeWithdrawLimitClient = Substitute.For<IWithdrawLimitClient>();
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Id = brandId,
				Code = "GPI"
			});

		var walletId = Guid.NewGuid();
		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
			Arg.Any<string>(),
			Arg.Any<CancellationToken>())
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = walletId.ToString()
			}.ToImmutableDictionary());

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

		_ = fakePlayerBucketClient.ReadBucketsAsync(
			Arg.Any<string>(),
			Arg.Any<CancellationToken>())
			.Returns(new ReadBucketResult(0M, null));

		_ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is("TestPlayer"), Arg.Any<CancellationToken>())
			.Returns(new Tgs.PlayerService.Client.PlayerInfo
			{
				PlayerCode = "TestPlayer",
				BrandId = brandId
			});

		_ = fakeVipLevelClient.GetBrandAllVipLevelsAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new[]
			{
				new Tgs.VipLevelService.Clients.VipLevelInfo(
					Guid.NewGuid(),
					1,
					brandId,
					0M,
					0M,
					0M,
					0M,
					true,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					0M,
					5,
					10000M)
			}.ToAsyncEnumerable());

		_ = fakeWithdrawLimitClient.IncreasePlayerWithdrawStatisticsAsync(
			Arg.Is("TestPlayer"),
			Arg.Any<DateOnly>(),
			Arg.Any<decimal>(),
			Arg.Any<CancellationToken>())
			.Returns(new WithdrawStatistics(1000M, 1));

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(
			services => services
				.AddSingleton(fakeCashServiceClient)
				.AddSingleton(fakePlayerServiceClient)
				.AddSingleton(fakePlayerBucketClient)
				.AddSingleton(fakeVipLevelClient)
				.AddSingleton(fakeWithdrawLimitClient)
				.AddSingleton(fakeBrandServiceClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

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
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeBrandServiceClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var brandId = Guid.NewGuid();
		var bankInfoId = Guid.NewGuid();
		var agentId = Guid.NewGuid();

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Id = brandId,
				Code = "GPI"
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
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeBrandServiceClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var brandId = Guid.NewGuid();
		var bankInfoId = Guid.NewGuid();
		var agentId = Guid.NewGuid();

		_ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
			.Returns(new BrandInfo
			{
				Id = brandId,
				Code = "GPI"
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
