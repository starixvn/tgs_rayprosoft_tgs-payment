using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Cache;

internal class AllCachedPaymentChannelsQueryService(
	IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo> sourceService,
	IMemoryCache memoryCache)
	: IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo>
	, IPaymentChannelCacheCleaner
{
	public void Clean(string channelCode)
	{
		var key = "Tgs:AllPaymentChannels";

		memoryCache.Remove(key);
	}

	public async IAsyncEnumerable<PaymentChannelInfo> QueryAsync(
		AllPaymentChannelsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var key = "Tgs:AllPaymentChannels";

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
