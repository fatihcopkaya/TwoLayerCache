using Instance1.Model;

namespace Instance1.Abstractions
{
    public interface ICacheService
    {
        public Task<T> GetAsync<T>(string key);
        public Task SetAsync<T>(string key, T item, Action<CacheSettings> settings);

    }


}
