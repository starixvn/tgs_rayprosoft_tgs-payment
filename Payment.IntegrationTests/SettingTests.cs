using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Legacy;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

public class SettingTests
{
	[Test]
	public async Task 設定Brand玩家銀行卡最高建立數量()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var brandId = Guid.NewGuid();

		await sut.SetBrandPlayerBankInfoCountLimitAsync(brandId, 10);
		var actual = dbContext.BrandPaymentSettings.FirstOrDefault();

		ClassicAssert.IsNotNull(actual);
		ClassicAssert.AreEqual(brandId, actual!.BrandId);
		ClassicAssert.AreEqual(10, actual.PlayerBankInfoCountLimit);
	}
}
