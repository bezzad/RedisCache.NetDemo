﻿using BenchmarkDotNet.Attributes;
using HybridRedisCache;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedisCache.Benchmark
{
    //[MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    public class BenchmarkManager
    {
        IMemoryCache _memCache;
        ICacheService _redisCache;
        EasyHybridCache _easyHybridCache;
        HybridCache _hybridCache;

        const int redisPort = 6379;
        const string redisIP = "127.0.0.1"; // "172.23.44.11"   "127.0.0.1" 
        const string KeyPrefix = "test_";
        const string ReadKeyPrefix = "test_x";
        const int ExpireDurationSecond = 3600;
        static SampleModel[] _data;
        static Lazy<SampleModel> _singleModel = new Lazy<SampleModel>(() => _data[0], true);
        static Lazy<SampleModel> _singleWorseModel = new Lazy<SampleModel>(() => _data[1], true);

        [Params(1, 10, 100)]
        public int RepeatCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Write your initialization code here
            var connection = ConnectionMultiplexer.Connect($"{redisIP}:{redisPort}");
            _redisCache = new CacheService(connection);
            _memCache = new MemoryCache(new MemoryCacheOptions());
            _easyHybridCache = new EasyHybridCache(redisIP, redisPort);
            _data ??= Enumerable.Range(0, 10000).Select(_ => SampleModel.Factory()).ToArray();
            _hybridCache = new HybridCache(new HybridCachingOptions()
            {
                InstanceName = nameof(BenchmarkManager),
                DefaultExpirationTime = TimeSpan.FromDays(1),
                RedisCacheConnectString = $"{redisIP}:{redisPort}",
                ThrowIfDistributedCacheError = false
            });
        }

        [Benchmark(Baseline = true)]
        public void Add_Memory()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                _memCache.Set(KeyPrefix + i, JsonSerializer.Serialize(_data[i]), DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));
        }

        [Benchmark]
        public async Task Add_Memory_Async()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                await _memCache.GetOrCreateAsync(KeyPrefix + i, _ => Task.FromResult(JsonSerializer.Serialize(_data[i])));
        }

        [Benchmark]
        public void Add_Redis()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                _redisCache.AddOrUpdate(KeyPrefix + i, _data[i], DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));
        }

        [Benchmark]
        public async Task Add_Redis_Async()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                await _redisCache.AddOrUpdateAsync(KeyPrefix + i, _data[i], DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));
        }

        [Benchmark]
        public void Add_FireAndForget_Redis()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                _redisCache.AddOrUpdate(KeyPrefix + i, _data[i], DateTimeOffset.Now.AddSeconds(ExpireDurationSecond), true);
        }

        [Benchmark]
        public async Task Add_FireAndForget_Redis_Async()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                await _redisCache.AddOrUpdateAsync(KeyPrefix + i, _data[i], DateTimeOffset.Now.AddSeconds(ExpireDurationSecond), true);
        }

        [Benchmark]
        public void Add_EasyHybrid()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                _easyHybridCache.Set(KeyPrefix + i, _data[i], TimeSpan.FromSeconds(ExpireDurationSecond));
        }

        [Benchmark]
        public async Task Add_EasyHybrid_Async()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                await _easyHybridCache.SetAsync(KeyPrefix + i, _data[i], TimeSpan.FromSeconds(ExpireDurationSecond));
        }

        [Benchmark]
        public void Add_Hybrid()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                _hybridCache.Set(KeyPrefix + i, _data[i], TimeSpan.FromSeconds(ExpireDurationSecond), true);
        }

        [Benchmark]
        public async Task Add_Hybrid_Async()
        {
            // write cache
            for (var i = 0; i < RepeatCount; i++)
                await _hybridCache.SetAsync(KeyPrefix + i, _data[i], TimeSpan.FromSeconds(ExpireDurationSecond), true);
        }

        [Benchmark]
        public void Get_Memory()
        {
            // write single cache
            _memCache.Set(ReadKeyPrefix, _singleModel.Value, DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));

            // read cache
            for (var i = 0; i < RepeatCount; i++)
                if (_memCache.TryGetValue(ReadKeyPrefix, out string value))
                    ThrowIfIsNotMatch(JsonSerializer.Deserialize<SampleModel>(value), _singleModel.Value);
        }

        [Benchmark]
        public async Task Get_Memory_Async()
        {
            // write single cache
            _memCache.Set(ReadKeyPrefix, JsonSerializer.Serialize(_singleModel.Value), DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));

            // read cache
            for (var i = 0; i < RepeatCount; i++)
            {
                // don't generate correct data when couldn't find, because its already wrote!
                var value = await _memCache.GetOrCreateAsync(ReadKeyPrefix, _ => Task.FromResult(JsonSerializer.Serialize(_singleWorseModel.Value)));
                ThrowIfIsNotMatch(JsonSerializer.Deserialize<SampleModel>(value), _singleModel.Value);
            }
        }

        [Benchmark]
        public void Get_Redis()
        {
            // write single cache
            _redisCache.AddOrUpdate(ReadKeyPrefix, _singleModel.Value, DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));

            // read cache
            for (var i = 0; i < RepeatCount; i++)
                if (_redisCache.TryGetValue(ReadKeyPrefix, out SampleModel value))
                    ThrowIfIsNotMatch(value, _singleModel.Value);
        }

        [Benchmark]
        public async Task Get_Redis_Async()
        {
            // write single cache
            await _redisCache.AddOrUpdateAsync(ReadKeyPrefix, _singleModel.Value, DateTimeOffset.Now.AddSeconds(ExpireDurationSecond));

            // read cache
            for (var i = 0; i < RepeatCount; i++)
            {
                // don't generate correct data when couldn't find, because its already wrote!
                var value = await _redisCache.GetAsync(ReadKeyPrefix, () => Task.FromResult(_singleWorseModel.Value), ExpireDurationSecond);
                ThrowIfIsNotMatch(value, _singleModel.Value);
            }
        }

        [Benchmark]
        public void Get_EasyHybrid()
        {
            // write single cache
            _easyHybridCache.Set(ReadKeyPrefix, _singleModel.Value, TimeSpan.FromSeconds(ExpireDurationSecond));

            // read cache
            for (var i = 0; i < RepeatCount; i++)
            {
                // don't generate correct data when couldn't find, because its already wrote!
                var value = _easyHybridCache.Get<SampleModel>(ReadKeyPrefix);
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
            }
        }

        [Benchmark]
        public async Task Get_EasyHybrid_Async()
        {
            // write single cache
            await _easyHybridCache.SetAsync(ReadKeyPrefix, _singleModel.Value, TimeSpan.FromSeconds(ExpireDurationSecond));

            // read cache
            for (var i = 0; i < RepeatCount; i++)
            {
                // don't generate correct data when couldn't find, because its already wrote!
                var value = await _easyHybridCache.GetAsync<SampleModel>(ReadKeyPrefix);
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
            }
        }

        [Benchmark]
        public void Get_Hybrid()
        {
            // write single cache
            _hybridCache.Set(ReadKeyPrefix, _singleModel.Value, TimeSpan.FromSeconds(ExpireDurationSecond), true);

            // read cache
            for (var i = 0; i < RepeatCount; i++)
            {
                // don't generate correct data when couldn't find, because its already wrote!
                var value = _easyHybridCache.Get<SampleModel>(ReadKeyPrefix);
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
            }
        }

        [Benchmark]
        public async Task Get_Hybrid_Async()
        {
            // write single cache
            await _hybridCache.SetAsync(ReadKeyPrefix, _singleModel.Value, TimeSpan.FromSeconds(ExpireDurationSecond), true);

            // read cache
            for (var i = 0; i < RepeatCount; i++)
            {
                // don't generate correct data when couldn't find, because its already wrote!
                var value = await _hybridCache.GetAsync<SampleModel>(ReadKeyPrefix);
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
            }
        }

        private void ThrowIfIsNotMatch(SampleModel a, SampleModel b)
        {
            if (a?.Id != b?.Id)
                throw new ArrayTypeMismatchException($"value.Id({a?.Id} not equal with _data[i].Id({b?.Id}");
        }
    }
}
