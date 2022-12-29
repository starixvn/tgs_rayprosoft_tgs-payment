using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Tgs.Payment.Repositories;

internal class PaymentRateLimitRepository : IPaymentRateLimitRepository
{
	private readonly IDatabase _database;

	public PaymentRateLimitRepository(RedisConnectionFactory redisConnectionFactory)
	{
		if (redisConnectionFactory is null)
			throw new ArgumentNullException(nameof(redisConnectionFactory));

		_database = redisConnectionFactory.Database;
	}

	public async ValueTask<Guid> LockPaymentAsync(
		Guid formId,
		string paymentCode,
		string playerCode,
		CancellationToken cancellationToken = default)
	{
		var key = $"Tgs:Payment:Locks:{paymentCode}:{playerCode}";

		var isLockSuccess = await _database
			.StringSetAsync(
				key,
				formId.ToString(),
				expiry: TimeSpan.FromHours(1D),
				when: When.NotExists)
			.ConfigureAwait(false);

		if (isLockSuccess)
			return formId;

		var existsFormId = await _database.StringGetAsync(key).ConfigureAwait(false);

		return Guid.Parse(existsFormId.ToString());
	}

	public async ValueTask UnlockPaymentAsync(string paymentCode, string playerCode, CancellationToken cancellationToken = default)
	{
		var key = $"Tgs:Payment:Locks:{paymentCode}:{playerCode}";

		var hasMutex = await _database.StringSetAsync(
			$"{key}:Delete",
			Environment.MachineName,
			expiry: TimeSpan.FromSeconds(10D),
			when: When.NotExists).ConfigureAwait(false);

		if (hasMutex)
		{
			_ = await _database.KeyDeleteAsync(key).ConfigureAwait(false);

			_ = await _database.KeyDeleteAsync($"{key}:Delete").ConfigureAwait(false);
		}
	}
}
