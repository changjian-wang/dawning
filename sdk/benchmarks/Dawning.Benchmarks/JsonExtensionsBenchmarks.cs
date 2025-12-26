using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dawning.Extensions;

namespace Dawning.Benchmarks;

/// <summary>
/// JSON 扩展方法基准测试
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class JsonExtensionsBenchmarks
{
    private TestObject _simpleObject = null!;
    private TestObject _complexObject = null!;
    private List<TestObject> _objectList = null!;
    private string _simpleJson = null!;
    private string _complexJson = null!;
    private string _listJson = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleObject = new TestObject
        {
            Id = 1,
            Name = "Test",
            Email = "test@example.com",
            CreatedAt = DateTime.Now,
        };

        _complexObject = new TestObject
        {
            Id = 1,
            Name = "Test",
            Email = "test@example.com",
            CreatedAt = DateTime.Now,
            Tags = new List<string> { "tag1", "tag2", "tag3" },
            Metadata = new Dictionary<string, object>
            {
                ["key1"] = "value1",
                ["key2"] = 123,
                ["key3"] = true,
            },
        };

        _objectList = Enumerable
            .Range(1, 100)
            .Select(i => new TestObject
            {
                Id = i,
                Name = $"Test{i}",
                Email = $"test{i}@example.com",
                CreatedAt = DateTime.Now.AddDays(-i),
            })
            .ToList();

        _simpleJson = _simpleObject.ToJson();
        _complexJson = _complexObject.ToJson();
        _listJson = _objectList.ToJson();
    }

    [Benchmark]
    public string Serialize_Simple() => _simpleObject.ToJson();

    [Benchmark]
    public string Serialize_Complex() => _complexObject.ToJson();

    [Benchmark]
    public string Serialize_List() => _objectList.ToJson();

    [Benchmark]
    public string Serialize_Indented() => _simpleObject.ToJson(indented: true);

    [Benchmark]
    public TestObject? Deserialize_Simple() => _simpleJson.FromJson<TestObject>();

    [Benchmark]
    public TestObject? Deserialize_Complex() => _complexJson.FromJson<TestObject>();

    [Benchmark]
    public List<TestObject>? Deserialize_List() => _listJson.FromJson<List<TestObject>>();

    [Benchmark]
    public TestObject? DeepClone() => _complexObject.DeepClone();

    [Benchmark]
    public bool IsValidJson_Valid() => _simpleJson.IsValidJson();

    [Benchmark]
    public bool IsValidJson_Invalid() => "invalid json".IsValidJson();

    [Benchmark]
    public string PrettyPrint() => _simpleJson.PrettyPrint();

    public class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string>? Tags { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
