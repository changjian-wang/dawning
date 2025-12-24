using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dawning.Extensions;

namespace Dawning.Benchmarks;

/// <summary>
/// 字符串扩展方法基准测试
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringExtensionsBenchmarks
{
    private const string TestString = "getUserInfoFromDatabase";
    private const string LongString = "This is a very long string that needs to be truncated to a shorter length for display purposes";
    private const string Email = "test.user@example.com";
    private const string Phone = "13800138000";
    private const string SensitiveData = "1234567890123456";

    [Benchmark]
    public string ToPascalCase() => TestString.ToPascalCase();

    [Benchmark]
    public string ToCamelCase() => "GetUserInfo".ToCamelCase();

    [Benchmark]
    public string ToSnakeCase() => TestString.ToSnakeCase();

    [Benchmark]
    public string ToKebabCase() => TestString.ToKebabCase();

    [Benchmark]
    public string Truncate() => LongString.Truncate(20);

    [Benchmark]
    public string TruncateWithEllipsis() => LongString.Truncate(20, "...");

    [Benchmark]
    public bool IsValidEmail() => Email.IsValidEmail();

    [Benchmark]
    public bool IsValidPhoneNumber() => Phone.IsValidPhoneNumber();

    [Benchmark]
    public string MaskString() => SensitiveData.Mask();

    [Benchmark]
    public bool IsNullOrEmpty() => TestString.IsNullOrEmpty();

    [Benchmark]
    public bool IsNullOrWhiteSpace() => TestString.IsNullOrWhiteSpace();
}
