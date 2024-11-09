#Cache Management System

Instance1 is a dual-layer caching system leveraging **Redis** for distributed caching and **In-Memory Cache** for local caching. It features output caching via a custom action filter and uses Redis Pub/Sub to synchronize cache updates across multiple instances, ensuring efficient data retrieval.

## Features

- **Dual-Layer Caching**: Combines in-memory and distributed (Redis) caching to improve data access performance.
- **Output Caching**: Uses the `OutPutCacheActionFilter` attribute for controller-level output caching.
- **Automatic Cache Synchronization**: Utilizes Redis Pub/Sub to synchronize cache updates across multiple instances.
- **Custom Expiration**: Allows for custom cache expiration times.

## Project Structure

- **ICacheService**: Defines the primary cache operations (`GetAsync`, `SetAsync`).
- **RedisService**: Implements `ICacheService` using Redis.
- **InMemoryCacheService**: Implements `ICacheService` using in-memory caching.
- **CacheProxyService**: Manages cache retrieval, first checking in-memory, then falling back to Redis if necessary.
- **CacheConsumerService**: Listens for Redis Pub/Sub messages to update the in-memory cache.
- **OutPutCacheActionFilter**: Checks the cache for responses to requests and caches responses when necessary.

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Channel": "CacheChannel",
    "TtlThresholdValue": 5000
  },
  "InMemoryCache": {
    "SizeLimit": 100,
    "DefaultTtlValue": 5
  }
}
