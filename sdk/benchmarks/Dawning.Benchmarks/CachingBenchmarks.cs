using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dawning.Caching;
using Dawning.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dawning.Benchmarks;

/// <summary>
/// 内存缓存服务基准测试
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CachingBenchmarks
{
    private MemoryCacheService _cacheService = null!;
    private TestObject _testObject = null!;
    private string _testKey = null!;

    [GlobalSetup]
    public void Setup()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(
            new CacheOptions { DefaultExpirationMinutes = 30, KeyPrefix = "bench" }
        );
        _cacheService = new MemoryCacheService(memoryCache, options);

        _testObject = new TestObject
        {
            Id = 1,
            Name = "Test Object",
            Data = Enumerable.Range(1, 100).ToList(),
        };

        _testKey = "benchmark-key";

        // Pre-populate cache for read tests
        _cacheService.SetAsync("existing-key", _testObject).Wait();
    }

    [Benchmark]
    public async Task SetAsync_SimpleValue()
    {
        await _cacheService.SetAsync($"key-{Guid.NewGuid()}", "simple-value");
    }

    [Benchmark]
    public async Task SetAsync_ComplexObject()
    {
        await _cacheService.SetAsync($"key-{Guid.NewGuid()}", _testObject);
    }

    [Benchmark]
    public async Task<TestObject?> GetAsync_Hit()
    {
        return await _cacheService.GetAsync<TestObject>("existing-key");
    }

    [Benchmark]
    public async Task<string?> GetAsync_Miss()
    {
        return await _cacheService.GetAsync<string>("non-existing-key");
    }

    [Benchmark]
    public async Task<bool> ExistsAsync_Hit()
    {
        return await _cacheService.ExistsAsync("existing-key");
    }

    [Benchmark]
    public async Task<bool> ExistsAsync_Miss()
    {
        return await _cacheService.ExistsAsync("non-existing-key");
    }

    [Benchmark]
    public async Task<TestObject?> GetOrSetAsync_Hit()
    {
        return await _cacheService.GetOrSetAsync(
            "existing-key",
            () => Task.FromResult(_testObject)
        );
    }

    [Benchmark]
    public async Task<TestObject?> GetOrSetAsync_Miss()
    {
        return await _cacheService.GetOrSetAsync(
            $"new-key-{Guid.NewGuid()}",
            () => Task.FromResult(_testObject)
        );
    }

    public class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<int> Data { get; set; } = new();
    }
}
