using BenchmarkDotNet.Attributes;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace RedisCache.Benchmark
{
    //[MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    public class BenchmarkManager
    {
        IMemoryCache _memCache;
        ICacheService _redisCache;
        const string KeyPrefix = "test_";

        [Params(1, 10, 100)]
        int RepeatCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Write your initialization code here
            var connection = ConnectionMultiplexer.Connect("172.23.44.11:6379");
            _redisCache = new CacheService(connection);
            _memCache = new MemoryCache(new MemoryCacheOptions());
        }

        [Benchmark(Baseline = true)]
        public void Add_LocalMemory()
        {
            for (var i = 0; i < RepeatCount; i++)
                _memCache.Set(KeyPrefix + i, i, DateTimeOffset.Now.AddMinutes(1));
        }

        [Benchmark]
        public void Add_Redis()
        {
            for (var i = 0; i < RepeatCount; i++)
                _redisCache.AddOrUpdate(KeyPrefix + i, i, DateTimeOffset.Now.AddMinutes(1));
        }

        [Benchmark]
        public async Task Add_Async_Redis()
        {
            for (var i = 0; i < RepeatCount; i++)
                await _redisCache.AddOrUpdateAsync(KeyPrefix + i, i, DateTimeOffset.Now.AddMinutes(1));
        }

        [Benchmark]
        public void GetOrCreate_Async_LocalMemory()
        {
            for (var i = 0; i < RepeatCount; i++)
                _memCache.GetOrCreateAsync(KeyPrefix + i, _=> Task.FromResult(i));
        }

        [Benchmark]
        public async Task Add_FireAndForget_Async_Redis()
        {
            for (var i = 0; i < RepeatCount; i++)
                await _redisCache.AddOrUpdateAsync(KeyPrefix + i, i, DateTimeOffset.Now.AddMinutes(1), true);
        }

        [Benchmark]
        public void Add_FireAndForget_Redis()
        {
            for (var i = 0; i < RepeatCount; i++)
                _redisCache.AddOrUpdate(KeyPrefix + i, i, DateTimeOffset.Now.AddMinutes(1), true);
        }
    }
}
