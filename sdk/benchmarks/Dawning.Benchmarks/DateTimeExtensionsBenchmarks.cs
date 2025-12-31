using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dawning.Extensions;

namespace Dawning.Benchmarks;

/// <summary>
/// DateTime extension methods benchmarks
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class DateTimeExtensionsBenchmarks
{
    private DateTime _testDate;
    private DateTime _birthday;
    private DateTime _pastDate;

    [GlobalSetup]
    public void Setup()
    {
        _testDate = new DateTime(2025, 12, 24, 14, 30, 45);
        _birthday = new DateTime(1990, 5, 15);
        _pastDate = DateTime.Now.AddDays(-5);
    }

    [Benchmark]
    public DateTime StartOfDay() => _testDate.StartOfDay();

    [Benchmark]
    public DateTime EndOfDay() => _testDate.EndOfDay();

    [Benchmark]
    public DateTime StartOfMonth() => _testDate.StartOfMonth();

    [Benchmark]
    public DateTime EndOfMonth() => _testDate.EndOfMonth();

    [Benchmark]
    public DateTime StartOfYear() => _testDate.StartOfYear();

    [Benchmark]
    public DateTime EndOfYear() => _testDate.EndOfYear();

    [Benchmark]
    public bool IsWeekend() => _testDate.IsWeekend();

    [Benchmark]
    public bool IsWeekday() => _testDate.IsWeekday();

    [Benchmark]
    public int CalculateAge() => _birthday.CalculateAge();

    [Benchmark]
    public string ToRelativeTime() => _pastDate.ToRelativeTime();

    [Benchmark]
    public DateTime AddWorkdays() => _testDate.AddWorkdays(5);

    [Benchmark]
    public int DaysBetween() => _pastDate.DaysBetween(DateTime.Now);

    [Benchmark]
    public long ToUnixTimeSeconds() => _testDate.ToUnixTimeSeconds();
}
