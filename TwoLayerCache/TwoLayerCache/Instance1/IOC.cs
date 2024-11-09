using Instance1.Abstractions;
using Instance1.Consumer;
using Instance1.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using StackExchange.Redis;

namespace Instance1
{
    public static class IOC
    {
        public static IServiceCollection ServiceRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache(x =>
            {
                x.SizeLimit = configuration.GetValue<int>("InMemoryCache:SizeLimit");
            });
            #region Redis ve ConnectionMultiplexer registration
            var redisOptions = new RedisCacheOptions
            {
                Configuration = configuration.GetSection("Redis")["ConnectionString"],
            };

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configOptions = ConfigurationOptions.Parse(redisOptions.Configuration);
                configOptions.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(configOptions);
            });
            services.AddKeyedSingleton<IDistributedCache, RedisCache>("RedisCache", (sp, _) => new RedisCache(redisOptions));

            #endregion

            #region ICacheService registration
            services.AddKeyedTransient<ICacheService, RedisService>("RedisService");

            services.AddKeyedTransient<ICacheService, InMemoryCacheService>("InMemoryCacheService");

            services.AddKeyedTransient<ICacheService, CacheProxyService>("CacheProxyService");
            #endregion

            services.AddHostedService<CacheConsumerService>();
            return services;
        }
    }
}
