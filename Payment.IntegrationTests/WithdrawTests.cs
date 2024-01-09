using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using Tgs.PlayerService.Client;

namespace Payment.IntegrationTests;

public class WithdrawTests
{
	[Test]
	public async Task 拒絕提款單後_表單應該為Rejected_並且會將申請金額Rollback回錢包()
	{
		var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeCashServiceClient)
			.AddSingleton(fakePlayerServiceClient));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var formId = Guid.NewGuid();
		var playerCode = "test1";
		var playerWalletId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		_ = await dbContext.WithdrawForms.AddAsync(new WithdrawForm
		{
			Id = formId,
			BrandId = Guid.NewGuid(),
			UserId = playerCode,
			UserName = "Player",
			UserType = (int)UserType.Player,
			Amount = 1000M,
			Status = (int)PaymentFlowState.WaitApprove,
			CreatedOn = DateTime.Now,
			UpdatedOn = DateTime.Now,
			BankInfo = "{}"
		});

		_ = await dbContext.SaveChangesAsync();

		_ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(new Dictionary<string, string>
			{
				["DefaultWalletId"] = playerWalletId.ToString()
			}.ToImmutableDictionary());

		await sut.ReviewWithdrawFormAsync(
			formId,
			null,
			ReviewState.Reject,
			null,
			null,
			null);
		var actual = await dbContext.WithdrawForms.FindAsync(formId);

		ClassicAssert.NotNull(actual);
		ClassicAssert.AreEqual((int)PaymentFlowState.Reject, actual.Status);

		_ = fakeCashServiceClient.Received(1)
			.RollbackWithdrawAsync(
				Arg.Is(playerWalletId),
				Arg.Is(1000M),
				Arg.Any<string>(),
				Arg.Is(formId.ToString()),
				Arg.Any<Guid?>(),
				Arg.Any<CancellationToken>());
	}
}
