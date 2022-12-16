using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Cache;

internal class AllCachedPaymentMethodChannelsQueryService : IQueryService<AllPaymentMethodChannelsQuery, PaymentMethodChannelInfo>, IPaymentMethodChannelCacheCleaner
{
	private readonly IMemoryCache _memoryCache;
	private readonly IQueryService<AllPaymentMethodChannelsQuery, PaymentMethodChannelInfo> _sourceService;

	public AllCachedPaymentMethodChannelsQueryService(IQueryService<AllPaymentMethodChannelsQuery, PaymentMethodChannelInfo> sourceService, IMemoryCache memoryCache)
	{
		_sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}

	public void Clean(Guid pmcId)
	{
		var key = "Tgs:AllPaymentMethodChannels";

		_memoryCache.Remove(key);
	}

	public async IAsyncEnumerable<PaymentMethodChannelInfo> QueryAsync(
			AllPaymentMethodChannelsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var key = "Tgs:AllPaymentMethodChannels";

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
