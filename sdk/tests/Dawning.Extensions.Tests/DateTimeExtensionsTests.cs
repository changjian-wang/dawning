using FluentAssertions;
using Xunit;

namespace Dawning.Extensions.Tests;

public class DateTimeExtensionsTests
{
    [Fact]
    public void StartOfDay_ShouldReturnMidnight()
    {
        var date = new DateTime(2024, 1, 15, 14, 30, 45);
        var result = date.StartOfDay();

        result.Should().Be(new DateTime(2024, 1, 15, 0, 0, 0));
    }

    [Fact]
    public void EndOfDay_ShouldReturnEndOfDay()
    {
        var date = new DateTime(2024, 1, 15, 14, 30, 45);
        var result = date.EndOfDay();

        result.Hour.Should().Be(23);
        result.Minute.Should().Be(59);
        result.Second.Should().Be(59);
    }

    [Fact]
    public void StartOfMonth_ShouldReturnFirstDayOfMonth()
    {
        var date = new DateTime(2024, 1, 15, 14, 30, 45);
        var result = date.StartOfMonth();

        result.Should().Be(new DateTime(2024, 1, 1, 0, 0, 0));
    }

    [Fact]
    public void EndOfMonth_ShouldReturnLastDayOfMonth()
    {
        var date = new DateTime(2024, 1, 15);
        var result = date.EndOfMonth();

        result.Day.Should().Be(31);
        result.Month.Should().Be(1);
    }

    [Fact]
    public void StartOfYear_ShouldReturnFirstDayOfYear()
    {
        var date = new DateTime(2024, 6, 15);
        var result = date.StartOfYear();

        result.Should().Be(new DateTime(2024, 1, 1, 0, 0, 0));
    }

    [Theory]
    [InlineData(2024, 1, 13, true)] // Saturday
    [InlineData(2024, 1, 14, true)] // Sunday
    [InlineData(2024, 1, 15, false)] // Monday
    public void IsWeekend_ShouldReturnCorrectResult(int year, int month, int day, bool expected)
    {
        var date = new DateTime(year, month, day);
        date.IsWeekend().Should().Be(expected);
    }

    [Theory]
    [InlineData(2024, 1, 15, true)] // Monday
    [InlineData(2024, 1, 13, false)] // Saturday
    public void IsWeekday_ShouldReturnCorrectResult(int year, int month, int day, bool expected)
    {
        var date = new DateTime(year, month, day);
        date.IsWeekday().Should().Be(expected);
    }

    [Fact]
    public void DaysBetween_ShouldCalculateCorrectly()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 10);

        start.DaysBetween(end).Should().Be(9);
    }

    [Fact]
    public void CalculateAge_ShouldCalculateCorrectly()
    {
        var birthDate = new DateTime(2000, 6, 15);
        var reference = new DateTime(2024, 6, 14);

        birthDate.CalculateAge(reference).Should().Be(23);

        var referenceAfterBirthday = new DateTime(2024, 6, 15);
        birthDate.CalculateAge(referenceAfterBirthday).Should().Be(24);
    }

    [Fact]
    public void ToUnixTimeSeconds_ShouldConvertCorrectly()
    {
        var date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var timestamp = date.ToUnixTimeSeconds();

        timestamp.Should().Be(1704067200);
    }

    [Fact]
    public void FromUnixTimeSeconds_ShouldConvertCorrectly()
    {
        var timestamp = 1704067200L;
        var date = timestamp.FromUnixTimeSeconds();

        date.Year.Should().Be(2024);
        date.Month.Should().Be(1);
        date.Day.Should().Be(1);
    }

    [Fact]
    public void ToDateString_ShouldFormatCorrectly()
    {
        var date = new DateTime(2024, 1, 15);
        date.ToDateString().Should().Be("2024-01-15");
    }

    [Fact]
    public void ToDateTimeString_ShouldFormatCorrectly()
    {
        var date = new DateTime(2024, 1, 15, 10, 30, 45);
        date.ToDateTimeString().Should().Be("2024-01-15 10:30:45");
    }

    [Fact]
    public void SetTime_ShouldSetTimeCorrectly()
    {
        var date = new DateTime(2024, 1, 15, 14, 30, 45);
        var result = date.SetTime(10, 15, 30);

        result.Hour.Should().Be(10);
        result.Minute.Should().Be(15);
        result.Second.Should().Be(30);
        result.Day.Should().Be(15);
    }

    [Fact]
    public void AddWorkdays_ShouldSkipWeekends()
    {
        // Friday
        var friday = new DateTime(2024, 1, 12);
        var result = friday.AddWorkdays(1);

        // Should be Monday
        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.Day.Should().Be(15);
    }
}
