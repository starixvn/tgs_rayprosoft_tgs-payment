using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Tgs.Payment.Repositories;

internal class BrandNetDepositRepository : IBrandNetDepositRepository
{
	private static readonly string _KeyPrefix = "TGS:Payment:NetDeposit";
	private readonly IDatabase _redisDatabase;

	public BrandNetDepositRepository(RedisConnectionFactory redisConnectionFactory)
	{
		_redisDatabase = redisConnectionFactory.Database;
	}

	public async ValueTask CountBrandCurrentDayNetDepositAsync(Guid brandId, double amount)
	{
		var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8D));
		var expired = new DateTimeOffset(now.Date.AddDays(1), TimeSpan.FromHours(8D)).AddHours(12D);

		var key = $"{_KeyPrefix}:{brandId}:{now:yyyyMMdd}";

		_ = await _redisDatabase.StringSetAsync(
			key,
			0D,
			expiry: expired - now,
			when: When.NotExists).ConfigureAwait(false);

		_ = await _redisDatabase
			.StringIncrementAsync(key, amount)
			.ConfigureAwait(false);
	}

	public async ValueTask<decimal> GetBrandCurrentDayNetDepositAsync(Guid brandId)
	{
		var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8));
		var key = $"{_KeyPrefix}:{brandId}:{now:yyyyMMdd}";

		var netDeposit = await _redisDatabase
			.StringGetAsync(key)
			.ConfigureAwait(false);

		return (decimal)netDeposit;
	}

	public async ValueTask<decimal> GetBrandYesterDayNetDepositAsync(Guid brandId)
	{
		var yesterday = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8)).AddDays(-1);
		var key = $"{_KeyPrefix}:{brandId}:{yesterday:yyyyMMdd}";

		var netDeposit = await _redisDatabase
			.StringGetAsync(key)
			.ConfigureAwait(false);

		return (decimal)netDeposit;
	}
}
