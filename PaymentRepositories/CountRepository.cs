using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Tgs.Payment.Repositories;

internal class CountRepository : ICountRepository
{
	private static readonly string _KeyPrefix = "TGS:Payment:NetDeposit";
	private readonly IDatabase _redisDatabase;

	public CountRepository(RedisConnectionFactory redisConnectionFactory)
	{
		_redisDatabase = redisConnectionFactory.Database;
	}

	public async ValueTask CountAmountAsync(Guid brandId, double amount)
	{
		var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8));
		var key = $"{_KeyPrefix}:{brandId}:{now:yyyyMMdd}";

		_ = await _redisDatabase.StringSetAsync(
			key,
			0D,
			expiry: TimeSpan.FromDays(1.5D),
			when: When.NotExists).ConfigureAwait(false);

		_ = await _redisDatabase
			.StringIncrementAsync(key, amount)
			.ConfigureAwait(false);
	}
}
