using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Tgs.Payment.Repositories
{
	internal class RedisTakeNumberStore : ITakeNumberStore
	{
		private static readonly TimeSpan _Expire = TimeSpan.FromHours(2);
		private static readonly string _KeyPrefix = "TGS:Payment";
		private readonly IDatabase _redisDatabase;

		public RedisTakeNumberStore(RedisConnectionFactory redisConnectionFactory)
		{
			_redisDatabase = redisConnectionFactory.Database;
		}

		public async ValueTask<string> GetDepositFormOrderNoAsync(string BrandCode)
		{
			var dateRule = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8)).ToString("yyMMddHH");
			var dataKey = $"{_KeyPrefix}:{BrandCode}T{dateRule}";

			var serialNumber = await _redisDatabase.StringIncrementAsync(dataKey).ConfigureAwait(false);
			await _redisDatabase.KeyExpireAsync(dataKey, _Expire);

			return $"{BrandCode}T{dateRule}{serialNumber:D4}";
		}

		public async ValueTask<string> GetWithdrawFormOrderNoAsync(string BrandCode)
		{
			var dateRule = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8)).ToString("yyMMddHH");
			var dataKey = $"{_KeyPrefix}:{BrandCode}W{dateRule}";

			var serialNumber = await _redisDatabase.StringIncrementAsync(dataKey).ConfigureAwait(false);
			await _redisDatabase.KeyExpireAsync(dataKey, _Expire);

			return $"{BrandCode}W{dateRule}{serialNumber:D4}";
		}
	}
}
