using Tgs.Payment.Clients;

namespace Payment.IntegrationTests;

public class WithdrawFormTests
{
	[Test]
	public async Task 查詢WithdrawForms給FixTool使用()
	{
		await using TestPaymentApplication web = TestSiteHelper.GetPaymentServiceTestHost();

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var userType = UserType.Player;

		_ = await sut.QueryWithdrawFormsAsync(brandId, playerCode, userType).ToArrayAsync();
	}
}
