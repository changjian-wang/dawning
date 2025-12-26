using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dawning.Extensions;

namespace Dawning.Benchmarks;

/// <summary>
/// 集合扩展方法基准测试
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CollectionExtensionsBenchmarks
{
    private List<int> _smallList = null!;
    private List<int> _mediumList = null!;
    private List<int> _largeList = null!;
    private List<TestItem> _itemList = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallList = Enumerable.Range(1, 100).ToList();
        _mediumList = Enumerable.Range(1, 1000).ToList();
        _largeList = Enumerable.Range(1, 10000).ToList();
        _itemList = Enumerable
            .Range(1, 1000)
            .Select(i => new TestItem { Id = i, Category = $"Cat{i % 10}" })
            .ToList();
    }

    [Benchmark]
    public List<IEnumerable<int>> Batch_Small() => _smallList.Batch(10).ToList();

    [Benchmark]
    public List<IEnumerable<int>> Batch_Medium() => _mediumList.Batch(100).ToList();

    [Benchmark]
    public List<IEnumerable<int>> Batch_Large() => _largeList.Batch(1000).ToList();

    [Benchmark]
    public List<TestItem> DistinctBy() =>
        Dawning.Extensions.CollectionExtensions.DistinctBy(_itemList, x => x.Category).ToList();

    [Benchmark]
    public string JoinToString_Small() => _smallList.JoinToString(",");

    [Benchmark]
    public string JoinToString_Medium() => _mediumList.JoinToString(",");

    [Benchmark]
    public void ForEach_Small()
    {
        var sum = 0;
        _smallList.ForEach(x => sum += x);
    }

    [Benchmark]
    public void ForEach_Medium()
    {
        var sum = 0;
        _mediumList.ForEach(x => sum += x);
    }

    [Benchmark]
    public int? RandomElement() => _mediumList.RandomElement();

    [Benchmark]
    public List<int> Shuffle() => _smallList.Shuffle().ToList();

    public class TestItem
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
