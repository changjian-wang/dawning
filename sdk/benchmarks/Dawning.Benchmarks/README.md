# Dawning SDK Benchmarks

使用 [BenchmarkDotNet](https://benchmarkdotnet.org/) 进行 SDK 性能测试。

## 包含的基准测试

| 基准测试类 | 测试内容 |
|----------|---------|
| `StringExtensionsBenchmarks` | 字符串扩展方法（大小写转换、截断、验证等） |
| `CollectionExtensionsBenchmarks` | 集合扩展方法（批处理、去重、遍历等） |
| `DateTimeExtensionsBenchmarks` | 日期时间扩展方法（日期计算、格式化等） |
| `JsonExtensionsBenchmarks` | JSON 序列化/反序列化性能 |
| `CachingBenchmarks` | 内存缓存服务性能 |

## 运行基准测试

### 运行所有基准测试
```bash
cd sdk/benchmarks/Dawning.Benchmarks
dotnet run -c Release
```

### 运行特定基准测试类
```bash
# 只运行字符串扩展方法基准测试
dotnet run -c Release --filter "*String*"

# 只运行 JSON 基准测试
dotnet run -c Release --filter "*Json*"

# 只运行缓存基准测试
dotnet run -c Release --filter "*Caching*"
```

### 快速测试模式（用于验证）
```bash
dotnet run -c Release -- --job short
```

### 导出结果
```bash
# 导出为 HTML 和 Markdown
dotnet run -c Release --exporters html markdown
```

## 示例输出

运行后会在 `BenchmarkDotNet.Artifacts` 目录生成详细报告，包含：

- 执行时间（Mean、Error、StdDev）
- 内存分配（Allocated）
- 性能排名（Rank）

```
| Method           | Mean      | Error     | StdDev    | Rank | Allocated |
|----------------- |----------:|----------:|----------:|-----:|----------:|
| ToPascalCase     |  45.23 ns |  0.432 ns |  0.361 ns |    1 |      48 B |
| ToCamelCase      |  52.18 ns |  0.618 ns |  0.548 ns |    2 |      56 B |
| ToSnakeCase      |  89.34 ns |  1.023 ns |  0.854 ns |    3 |      72 B |
| ...              |       ... |       ... |       ... |  ... |       ... |
```

## 添加新基准测试

1. 创建新的基准测试类，使用 `[MemoryDiagnoser]` 和 `[Benchmark]` 特性
2. 在 `[GlobalSetup]` 中初始化测试数据
3. 每个基准测试方法应该只测试一个操作

```csharp
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class MyBenchmarks
{
    private string _testData = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testData = "test data";
    }

    [Benchmark]
    public string MyOperation() => _testData.ToUpperInvariant();
}
```

## 注意事项

- 基准测试应在 **Release** 模式下运行以获得准确结果
- 运行时关闭其他占用 CPU 的程序
- 完整基准测试可能需要几分钟时间
