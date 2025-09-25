using AGD.Service.Services.Interfaces;
using StackExchange.Redis;

namespace AGD.Service.Services.Implement
{
    public class RedisTokenBlacklistService : ITokenBlacklistService
    {
        private readonly IDatabase _database;
        private const string Prefix = "blacklist:jwt:";

        public RedisTokenBlacklistService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddAsync(string jti, TimeSpan ttl)
        {
            if (ttl <= TimeSpan.Zero)
            {
                return;
            }
            await _database.StringSetAsync(Prefix + jti, "1", ttl);
        }

        public async Task<bool> ExistsAsync(string jti)
        {
            return await _database.KeyExistsAsync(Prefix + jti);
        }
    }
}
