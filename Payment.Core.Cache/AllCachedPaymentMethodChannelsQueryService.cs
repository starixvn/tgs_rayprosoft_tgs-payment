using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Cache;

internal class AllCachedPaymentMethodChannelsQueryService(
	IQueryService<AllPaymentMethodChannelsQuery, PaymentMethodChannelInfo> sourceService,
	IMemoryCache memoryCache)
	: IQueryService<AllPaymentMethodChannelsQuery, PaymentMethodChannelInfo>, IPaymentMethodChannelCacheCleaner
{
	public void Clean(Guid pmcId)
	{
		var key = "Tgs:AllPaymentMethodChannels";

		memoryCache.Remove(key);
	}

	public async IAsyncEnumerable<PaymentMethodChannelInfo> QueryAsync(
			AllPaymentMethodChannelsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var key = "Tgs:AllPaymentMethodChannels";

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
