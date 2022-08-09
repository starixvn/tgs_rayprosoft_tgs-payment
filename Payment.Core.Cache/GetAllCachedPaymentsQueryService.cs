using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.ServiceUtility;
using Tgs.Payment.Cache;

namespace Tgs.Payment.Cache;

internal class GetAllCachedPaymentsQueryService : IQueryService<GetAllPaymentsQuery, PaymentInfo>, IPaymentCacheCleaner
{
	private readonly IMemoryCache _memoryCache;
	private readonly IQueryService<GetAllPaymentsQuery, PaymentInfo> _sourceService;

	public GetAllCachedPaymentsQueryService(IQueryService<GetAllPaymentsQuery, PaymentInfo> sourceService, IMemoryCache memoryCache)
	{
		_sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}

	public void Clean(string paymentCode)
	{
		var key = "Tgs:AllPayments";

		_memoryCache.Remove(key);
	}

	public async IAsyncEnumerable<PaymentInfo> QueryAsync(
		GetAllPaymentsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var key = "Tgs:AllPayments";

		var result = await _memoryCache.GetOrCreateAsync(key, async entry =>
		{
			var data = await _sourceService
				.QueryAsync(query, cancellationToken)
				.ToArrayAsync(cancellationToken)
				.ConfigureAwait(false);

			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8D);

			return data;
		}).ConfigureAwait(false);

		foreach (var item in result)
			yield return item;
	}
}
