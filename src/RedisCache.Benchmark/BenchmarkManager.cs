using BenchmarkDotNet.Attributes;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RedisCache.Benchmark
{
    //[MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    public class BenchmarkManager
    {
        ICacheService _cache;
        const string KeyPrefix = "test_";

        [Params(1, 10, 100)]
        int RepeatCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Write your initialization code here
            var connection = ConnectionMultiplexer.Connect("172.23.44.11:6379");
            _cache = new CacheService(connection);
        }

        [Benchmark]
        public async Task AddRedisAsync()
        {
            for (var i = 0; i < RepeatCount; i++)
            {
                await _cache.AddOrUpdateAsync(KeyPrefix + i, i, DateTimeOffset.Now.AddMinutes(1));
            }
        }

        [Benchmark]
        public void AddRedis()
        {
            for (var i = 0; i < RepeatCount; i++)
            {
                _cache.AddOrUpdate(KeyPrefix + i, i, DateTimeOffset.Now.AddMinutes(1));
            }
        }

        [Benchmark]
        public async Task AddRedisWithFireAndForgetAsync()
        {
            for (var i = 0; i < RepeatCount; i++)
            {
                await _cache.AddOrUpdateAsync(KeyPrefix + i, i, DateTimeOffset.Now.AddMinutes(1), true);
            }
        }

        [Benchmark]
        public void AddRedisWithFireAndForget()
        {
            for (var i = 0; i < RepeatCount; i++)
            {
                _cache.AddOrUpdate(KeyPrefix + i, i, DateTimeOffset.Now.AddMinutes(1), true);
            }
        }
    }
}
