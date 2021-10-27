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

		public async ValueTask<string> GetOrderNoAsync(string BrandCode, FormType formType)
		{
			var formTypeRule = formType == FormType.Deposit ? "T" : "W";
			var dateRule = DateTime.Now.ToString("yyMMddHH");
			var dataKey = $"{_KeyPrefix}:{BrandCode}{formTypeRule}{dateRule}";

			var serialNumber = await _redisDatabase.StringIncrementAsync(dataKey).ConfigureAwait(false);
			await _redisDatabase.KeyExpireAsync(dataKey, _Expire);

			return $"{BrandCode}{formTypeRule}{dateRule}{serialNumber.ToString().PadLeft(4, '0')}";
		}
	}
}
