using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Cache;

internal class AllCachedPaymentMethodsQueryService : IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo>, IPaymentMethodCacheCleaner
{
	private readonly IMemoryCache _memoryCache;
	private readonly IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo> _sourceService;

	public AllCachedPaymentMethodsQueryService(IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo> sourceService, IMemoryCache memoryCache)
	{
		_sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}

	public void Clean(string name)
	{
		var key = "Tgs:AllPaymentMethods";

		_memoryCache.Remove(key);
	}

	public async IAsyncEnumerable<PaymentMethodInfo> QueryAsync(
			AllPaymentMethodsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var key = "Tgs:AllPaymentMethods";

		var result = await _memoryCache.GetOrCreateAsync(key, async entry =>
		{
			var data = await _sourceService
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
