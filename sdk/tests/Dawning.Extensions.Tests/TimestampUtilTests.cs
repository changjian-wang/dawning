using Dawning.Extensions;
using FluentAssertions;

namespace Dawning.Extensions.Tests;

public class TimestampUtilTests
{
    [Fact]
    public void GetCurrentTimestamp_ShouldReturnUnixMilliseconds()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act
        var timestamp = TimestampUtil.GetCurrentTimestamp();

        // Assert
        var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        timestamp.Should().BeGreaterThanOrEqualTo(before);
        timestamp.Should().BeLessThanOrEqualTo(after);
    }

    [Fact]
    public void GetCurrentTimestamp_ShouldReturnPositiveValue()
    {
        // Act
        var timestamp = TimestampUtil.GetCurrentTimestamp();

        // Assert
        timestamp.Should().BePositive();
    }

    [Fact]
    public void GetCurrentTimestamp_CalledTwice_SecondShouldBeGreaterOrEqual()
    {
        // Act
        var first = TimestampUtil.GetCurrentTimestamp();
        var second = TimestampUtil.GetCurrentTimestamp();

        // Assert
        second.Should().BeGreaterThanOrEqualTo(first);
    }
}
