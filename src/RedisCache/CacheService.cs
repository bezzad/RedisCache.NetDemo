using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedisCache
{
    public class CacheService : ICacheService
    {
        private IDatabase _db;
        const string RedisChangeHandlerChannel = "RedisChangeHandlerChannel";

        public CacheService(IConnectionMultiplexer connection)
        {
            _db = connection.GetDatabase();
            var subscriber = connection.GetSubscriber();
            subscriber.SubscribeAsync(RedisChangeHandlerChannel, (channel, message) =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]: {$"Message {message} received successfully"}");
            });
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> acquire, int expireAfterSeconds)
        {
            if (TryGetValue(key, out T value) == false)
            {
                var expiryTime = TimeSpan.FromSeconds(expireAfterSeconds);
                value = await acquire();
                _db.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
            }

            return value;
        }

        public T Get<T>(string key, Func<T> acquire, int expireAfterSeconds)
        {
            if (TryGetValue(key, out T value) == false)
            {
                var expiryTime = TimeSpan.FromSeconds(expireAfterSeconds);
                value = acquire();
                _db.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
            }

            return value;
        }

        public T Get<T>(string key)
        {
            TryGetValue(key, out T value);
            return value;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var cacheValue = _db.StringGet(key);
            if (string.IsNullOrWhiteSpace(cacheValue) == false)
            {
                value = JsonSerializer.Deserialize<T>(cacheValue);
                return true;
            }

            value = default;
            return false;
        }

        public bool AddOrUpdate<T>(string key, T value, DateTimeOffset expirationTime, bool fireAndForget = false)
        {
            TimeSpan expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            var isSet = _db.StringSet(key, JsonSerializer.Serialize(value), expiryTime, When.Always,
                fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            _db.Publish(RedisChangeHandlerChannel, key);
            return isSet;
        }

        public async Task<bool> AddOrUpdateAsync<T>(string key, T value, DateTimeOffset expirationTime, bool fireAndForget = false)
        {
            TimeSpan expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            var result = await _db.StringSetAsync(key, JsonSerializer.Serialize(value), expiryTime, When.Always,
                fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);

            await _db.PublishAsync(RedisChangeHandlerChannel, key);
            return result;
        }

        public object Remove(string key)
        {
            bool _isKeyExist = _db.KeyExists(key);
            if (_isKeyExist == true)
            {
                _db.Publish(RedisChangeHandlerChannel, key);
                return _db.KeyDelete(key);
            }
            return false;
        }

        public void Clear()
        {
            _db.Execute("FLUSHDB");
        }

        public Task ClearAsync()
        {
            return _db.ExecuteAsync("FLUSHDB");
        }
    }
}
