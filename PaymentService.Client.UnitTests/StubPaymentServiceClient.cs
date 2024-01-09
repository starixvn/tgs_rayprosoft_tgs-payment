using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Tgs.Payment.Clients;

namespace PaymentService.Client.UnitTests;

internal class StubPaymentServiceClient(HttpClient httpClient) : PaymentServiceClient(httpClient)
{
	private readonly ConcurrentBag<Guid> _brandPaymentIds = [];

	public IEnumerable<Guid> CalledBrandPaymentIds => _brandPaymentIds.ToArray();

	protected override IAsyncEnumerable<BrandPaymentVip> QueryBrandPaymentVipsAsync(IEnumerable<Guid> brandPaymentIds, CancellationToken cancellationToken)
	{
		foreach (var id in brandPaymentIds)
			_brandPaymentIds.Add(id);

		return Array.Empty<BrandPaymentVip>().ToAsyncEnumerable();
	}
}
