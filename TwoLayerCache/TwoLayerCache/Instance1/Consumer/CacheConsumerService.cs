
using Instance1.Abstractions;
using Instance1.Model;
using StackExchange.Redis;
using System.Text.Json;

namespace Instance1.Consumer
{
    public class CacheConsumerService : BackgroundService
    {
        private readonly ICacheService _inMemoryCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;
        public CacheConsumerService([FromKeyedServices("InMemoryCacheService")] ICacheService inMemoryCache,
             IConnectionMultiplexer connectionMultiplexer, IConfiguration configuration)
        {
            _inMemoryCache = inMemoryCache;
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           await _connectionMultiplexer.GetSubscriber().SubscribeAsync(_configuration.GetValue<string>("Redis:Channel"), async (channel, message) =>
            {
                var redisMessage = JsonSerializer.Deserialize<CachePupSubMessage>(message);
                await _inMemoryCache.SetAsync(redisMessage.Key, redisMessage.Value, x =>
                {
                    x.ExpiryTime = _configuration.GetValue<int>("InMemoryCache:DefaulTtlValue");
                });
            });
            
        }
    }
}
