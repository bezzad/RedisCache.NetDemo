using BenchmarkDotNet.Attributes;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace RedisCache.Benchmark
{
    //[MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    public class BenchmarkManager
    {
        IMemoryCache _memCache;
        ICacheService _redisCache;
        const string KeyPrefix = "test_";
        const int ExpireDurationSecond = 3600;
        SampleModel[] _data;

        [Params(1, 10, 100)]
        public int RepeatCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Write your initialization code here
            var connection = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _redisCache = new CacheService(connection);
            _memCache = new MemoryCache(new MemoryCacheOptions());
            _data = Enumerable.Range(0, 10000).Select(_ => SampleModel.Factory()).ToArray();
        }

        [Benchmark(Baseline = true)]
        public void AddGet_Memory()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                _memCache.Set(KeyPrefix + i, _data[i], DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));

            // read cache
            for (var i = 0; i < RepeatCount; i++)
                if (_memCache.TryGetValue(KeyPrefix + i, out SampleModel value))
                    ThrowIfIsNotMatch(value, i);
        }

        [Benchmark]
        public async Task AddGet_Memory_Async()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                await _memCache.GetOrCreateAsync(KeyPrefix + i, _ => Task.FromResult(_data[i]));


            // read cache
            for (var i = 0; i < RepeatCount; i++)
            {
                var value = await _memCache.GetOrCreateAsync(KeyPrefix + i, _ => Task.FromResult(_data[i]));
                ThrowIfIsNotMatch(value, i);
            }
        }

        [Benchmark]
        public void AddGet_Redis()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                _redisCache.AddOrUpdate(KeyPrefix + i, _data[i], DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));

            // read cache
            for (var i = 0; i < RepeatCount; i++)
                if (_redisCache.TryGetValue(KeyPrefix + i, out SampleModel value))
                    ThrowIfIsNotMatch(value, i);
        }

        [Benchmark]
        public async Task AddGet_Redis_Async()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                await _redisCache.AddOrUpdateAsync(KeyPrefix + i, _data[i], DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));

            // read cache
            for (var i = 0; i < RepeatCount; i++)
            {
                var value = await _redisCache.GetAsync(KeyPrefix + i, () => Task.FromResult(_data[i]), ExpireDurationSecond);
                ThrowIfIsNotMatch(value, i);
            }
        }

        [Benchmark]
        public void AddGet_FireAndForget_Redis()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                _redisCache.AddOrUpdate(KeyPrefix + i, _data[i], DateTimeOffset.Now.AddSeconds(ExpireDurationSecond), true);

            // read cache
            for (var i = 0; i < RepeatCount; i++)
                if (_redisCache.TryGetValue(KeyPrefix + i, out SampleModel value))
                    ThrowIfIsNotMatch(value, i);
        }

        [Benchmark]
        public async Task AddGet_FireAndForget_Redis_Async()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                await _redisCache.AddOrUpdateAsync(KeyPrefix + i, _data[i], DateTimeOffset.Now.AddSeconds(ExpireDurationSecond), true);

            // read cache
            for (var i = 0; i < RepeatCount; i++)
            {
                var value = await _redisCache.GetAsync(KeyPrefix + i, () => Task.FromResult(_data[i]), ExpireDurationSecond);
                ThrowIfIsNotMatch(value, i);
            }
        }

        private void ThrowIfIsNotMatch(SampleModel a, int index)
        {
            if (a.Id != _data[index].Id)
                throw new ArrayTypeMismatchException($"value.Id({a.Id} not equal with _data[{index}].Id({_data[index].Id}");
        }
    }
}
