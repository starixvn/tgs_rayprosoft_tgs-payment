using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Cache;

internal class AllCachedPaymentChannelsQueryService : IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo>, IPaymentChannelCacheCleaner
{
	private readonly IMemoryCache _memoryCache;
	private readonly IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo> _sourceService;

	public AllCachedPaymentChannelsQueryService(IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo> sourceService, IMemoryCache memoryCache)
	{
		_sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}

	public void Clean(string channelCode)
	{
		var key = "Tgs:AllPaymentChannels";

		_memoryCache.Remove(key);
	}

	public async IAsyncEnumerable<PaymentChannelInfo> QueryAsync(
		AllPaymentChannelsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var key = "Tgs:AllPaymentChannels";

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
