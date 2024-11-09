
using Instance1.Abstractions;
using Instance1.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Instance1.Services
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _inMemoryCache;
        public InMemoryCacheService(IMemoryCache inMemoryCache)
        {
            _inMemoryCache = inMemoryCache;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (this._inMemoryCache.TryGetValue(key, out string value))
            {
                if (typeof(T).IsValueType)
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                else
                {
                    return JsonSerializer.Deserialize<T>(value);
                }
            }
            return default(T);
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

            long itemSizeInBytes = System.Text.Encoding.UTF8.GetByteCount(itemStringRepresentation);

            // eğer yeni veri eklenirse toplam boyut limiti aşacaksa, yazma işleminden kaçının
            var cacheSettings = new CacheSettings();
            settings(cacheSettings);
            await Task.Run(new Action(() =>
            {
                 _inMemoryCache.Set(key, itemStringRepresentation, new MemoryCacheEntryOptions

                 {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(cacheSettings.ExpiryTime),
                    Size = itemSizeInBytes

                 });
            }));

        }
    }
}
