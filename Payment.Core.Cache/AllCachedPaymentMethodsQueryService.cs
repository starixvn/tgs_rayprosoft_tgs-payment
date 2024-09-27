using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Cache;

internal class AllCachedPaymentMethodsQueryService(
	IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo> sourceService,
	IMemoryCache memoryCache)
	: IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo>
	, IPaymentMethodCacheCleaner
{
	public void Clean(string name)
	{
		var key = "Tgs:AllPaymentMethods";

		memoryCache.Remove(key);
	}

	public async IAsyncEnumerable<PaymentMethodInfo> QueryAsync(
			AllPaymentMethodsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var key = "Tgs:AllPaymentMethods";

		var result = await memoryCache.GetOrCreateAsync(key, async entry =>
		{
			var data = await sourceService
				.QueryAsync(query, cancellationToken)
				.ToArrayAsync(cancellationToken)
				.ConfigureAwait(false);

			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);

			return data;
		}).ConfigureAwait(false);

		foreach (var item in result!)
			yield return item;
	}
}
