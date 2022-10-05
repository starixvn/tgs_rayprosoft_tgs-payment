using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PaymentService.Client.UnitTests;

public class BrandPaymentTests
{
	[Test]
	public async Task Split_BrandPaymentId_For_QueryBrandPaymentVips_Should_Equivalent_Origin_Collection()
	{
		var ids = Enumerable.Range(0, 150)
			.Select(_ => Guid.NewGuid())
			.ToArray();

		var sut = new StubPaymentServiceClient(new HttpClient());

		_ = await sut.GetBrandPaymentVipsAsync(ids).ToArrayAsync();

		var actual = sut.CalledBrandPaymentIds.ToArray();

		CollectionAssert.AreEquivalent(ids, actual);
	}
}
