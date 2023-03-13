using StackExchange.Redis;
using System.Text.Json;

namespace RedisCacheDemo.Cache
{
    public class CacheService : ICacheService
    {
        private IDatabase _db;

        public CacheService(IConnectionMultiplexer connection)
        {
            _db = connection.GetDatabase();
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
            if(TryGetValue(key, out T value) == false)
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
            if(string.IsNullOrWhiteSpace(cacheValue) == false)
            {
                value = JsonSerializer.Deserialize<T>(cacheValue);
                return true;
            }

            value = default;
            return false;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            TimeSpan expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            var isSet = _db.StringSet(key, JsonSerializer.Serialize(value), expiryTime);

            return isSet;
        }

        public object Remove(string key)
        {
            bool _isKeyExist = _db.KeyExists(key);
            if (_isKeyExist == true)
            {
                return _db.KeyDelete(key);
            }
            return false;
        }

        public void Clear()
        {
            _db.Execute("FLUSHDB");
        }

        
    }
}
