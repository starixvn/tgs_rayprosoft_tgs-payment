using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.Payment.ApplicationServices.Statistics;
using Tgs.Payment;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;
using Tgs.ServiceUtility;
using NSubstitute;

namespace Payment.IntegrationTests;

public class QueryStatisticsTests
{
	[Test]
	public async Task 整合測試_查詢每日統計資料()
	{
		var fakeQueryService = Substitute.For<IQueryService<PaymentStatisticsQuery, UserPaymentSummaryStatistics>>();

		await using var web = TestSiteHelper.GetPaymentServiceTestHost(services => services
			.AddSingleton(fakeQueryService));

		var httpClient = web.CreateClient();

		var sut = new PaymentServiceClient(httpClient);

		var start = DateOnly.Parse("2024-1-29");
		var end = DateOnly.Parse("2024-1-31");
		var player = "player1";

		_ = fakeQueryService.QueryAsync(Arg.Any<PaymentStatisticsQuery>(), Arg.Any<CancellationToken>())
			.Returns(new[]
			{
				new UserPaymentSummaryStatistics(
					DateTime.UtcNow,
					player,
					Tgs.Payment.UserType.Player,
					100M,
					0M,
					1,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null)
			}.ToAsyncEnumerable());

		var actual = await sut.QueryPaymentStatisticsAsync(start, end, [player]).ToArrayAsync();

		Assert.That(actual, Is.Not.Null);
		Assert.That(actual.Length, Is.EqualTo(1));
	}
}
