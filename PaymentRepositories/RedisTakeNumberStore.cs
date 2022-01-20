using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Tgs.Payment.Repositories;

internal class RedisTakeNumberStore : ITakeNumberStore
{
	private static readonly TimeSpan _Expire = TimeSpan.FromHours(2);
	private static readonly string _KeyPrefix = "TGS:Payment";
	private readonly IDatabase _redisDatabase;

	public RedisTakeNumberStore(RedisConnectionFactory redisConnectionFactory)
	{
		_redisDatabase = redisConnectionFactory.Database;
	}

	public async ValueTask<string> GetDepositFormOrderNoAsync(string brandCode)
	{
		var dateRule = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8)).ToString("yyMMddHH");
		var dataKey = $"{_KeyPrefix}:{brandCode}T{dateRule}";

		var serialNumber = await _redisDatabase
			.StringIncrementAsync(dataKey)
			.ConfigureAwait(false);
		if (serialNumber == 1)
			_ = await _redisDatabase
				.KeyExpireAsync(dataKey, _Expire, CommandFlags.FireAndForget)
				.ConfigureAwait(false);

		return $"{brandCode}T{dateRule}{serialNumber:D4}";
	}

	public async ValueTask<string> GetWithdrawFormOrderNoAsync(string brandCode)
	{
		var dateRule = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8)).ToString("yyMMddHH");
		var dataKey = $"{_KeyPrefix}:{brandCode}W{dateRule}";

		var serialNumber = await _redisDatabase
			.StringIncrementAsync(dataKey)
			.ConfigureAwait(false);
		if (serialNumber == 1)
			_ = await _redisDatabase
				.KeyExpireAsync(dataKey, _Expire, CommandFlags.FireAndForget)
				.ConfigureAwait(false);

		return $"{brandCode}W{dateRule}{serialNumber:D4}";
	}
}
