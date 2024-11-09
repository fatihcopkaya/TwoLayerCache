
using Instance1.Abstractions;
using Instance1.Model;
using StackExchange.Redis;

namespace Instance1.Services
{
    public class CacheProxyService : ICacheService
    {
        private readonly ICacheService _inMemoryCache;
        private readonly ICacheService _redisCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public CacheProxyService(
            [FromKeyedServices("InMemoryCacheService")] ICacheService inMemoryCache,
            [FromKeyedServices("RedisService")] ICacheService redisCache,
            IConnectionMultiplexer connectionMultiplexer
            , IConfiguration configuration)
        {
            _inMemoryCache = inMemoryCache;
            _redisCache = redisCache;
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }


        public async Task<T> GetAsync<T>(string key)
        {
            var inMemoryValue = await _inMemoryCache.GetAsync<T>(key);
            if (inMemoryValue != null)
                return inMemoryValue;

            var redisValue = await _redisCache.GetAsync<T>(key);
            if (redisValue != null)
            {
                //Redisdeki keyin ttl değeri belirli bir Trashold valuenin üstünde ise inmemory'e yaz.
                if (await GetKeyTtlAsync(key) >= _configuration.GetValue<int>("Redis:TtlTrasholdValue"))
                {
                    await _inMemoryCache.SetAsync(key, redisValue, settings =>
                    {
                        settings.ExpiryTime = _configuration.GetValue<int>("InMemoryCache:DefaulTtlValue");
                    });
                   
                }
                return redisValue;
            }
            //redisde de bulamaz ise db ye gidilecek.
            return default(T);
        }

        public async Task SetAsync<T>(string key, T item, Action<CacheSettings> settings)
        {
            //sadece Redis'e set yapılacak Redis Pop/Sub özelliği ile ilgili channel'a pushlayacak Sub olan Instance alıp inmemorysine yazacak
            var cacheSettings = new CacheSettings();
            settings(cacheSettings);
            await _redisCache.SetAsync<T>(key, item, options =>
            {
               options.ExpiryTime = cacheSettings.ExpiryTime;
            });
            //db'ye yazılacak
        }

        private async Task<int> GetKeyTtlAsync(string key)
        {
            var database = _connectionMultiplexer.GetDatabase();

            TimeSpan? ttl = await database.KeyTimeToLiveAsync(key);
            if (ttl.HasValue)
            {
                return (int)ttl.Value.TotalMilliseconds;
            }

            return 0;
        }
    }
}
