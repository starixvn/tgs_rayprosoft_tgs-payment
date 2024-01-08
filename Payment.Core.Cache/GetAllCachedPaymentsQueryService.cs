using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Cache;

internal class GetAllCachedPaymentsQueryService(
	IQueryService<GetAllPaymentsQuery, PaymentInfo> sourceService,
	IMemoryCache memoryCache)
	: IQueryService<GetAllPaymentsQuery, PaymentInfo>
	, IPaymentCacheCleaner
{
	public void Clean(string paymentCode)
	{
		var key = "Tgs:AllPayments";

		memoryCache.Remove(key);
	}

	public async IAsyncEnumerable<PaymentInfo> QueryAsync(
		GetAllPaymentsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var key = "Tgs:AllPayments";

		var result = await memoryCache.GetOrCreateAsync(key, async entry =>
		{
			var data = await sourceService
				.QueryAsync(query, cancellationToken)
				.ToArrayAsync(cancellationToken)
				.ConfigureAwait(false);

			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8D);

			return data;
		}).ConfigureAwait(false);

		foreach (var item in result!)
			yield return item;
	}
}
