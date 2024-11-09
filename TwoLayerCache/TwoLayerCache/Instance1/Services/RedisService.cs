
using Instance1.Abstractions;
using Instance1.Model;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace Instance1.Services
{
    public class RedisService : ICacheService
    {
        private readonly IDistributedCache _redisCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public RedisService(
            [FromKeyedServices("RedisCache")] IDistributedCache redisCache,
            IConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _redisCache = redisCache;
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }
        public async Task<T> GetAsync<T>(string key)
        {
            var cacheItem = await _redisCache.GetStringAsync(key);

            if (cacheItem != null)
            {
                if (typeof(T).IsValueType)
                {
                    return (T)Convert.ChangeType(cacheItem, typeof(T));
                }
                else
                {
                    return JsonSerializer.Deserialize<T>(cacheItem);
                }
            }
            return default;
        }

        public async Task SetAsync<T>(string key, T item, Action<CacheSettings> settings)
        {
            string itemStringRepresentation;

            if (typeof(T).IsValueType)
            {
                itemStringRepresentation = item.ToString();
            }
            else
            {
                itemStringRepresentation = JsonSerializer.Serialize(item);
            }

            var cacheSettings = new CacheSettings();
            settings(cacheSettings);

            await _redisCache.SetStringAsync(key, itemStringRepresentation,new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(cacheSettings.ExpiryTime)
            });

            CachePopSubMessage message = new()
            {
                Key = key,
                Value = itemStringRepresentation
            };
           await PublishAsync(_configuration.GetValue<string>("Redis:Channel"), JsonSerializer.Serialize(message));
        }
        private async Task PublishAsync(string channel, string message)
        {
            var publisher = _connectionMultiplexer.GetSubscriber();
            await publisher.PublishAsync(channel, message);


        }
    }
}
