using Dawning.Resilience.Options;
using FluentAssertions;

namespace Dawning.Resilience.Tests;

public class ResilienceOptionsTests
{
    [Fact]
    public void RetryOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var options = new RetryOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.MaxRetryAttempts.Should().Be(3);
        options.BaseDelayMs.Should().Be(200);
        options.UseExponentialBackoff.Should().BeTrue();
        options.MaxDelayMs.Should().Be(30000);
    }

    [Fact]
    public void CircuitBreakerOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var options = new CircuitBreakerOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.SamplingDurationSeconds.Should().Be(30);
        options.FailureRatioThreshold.Should().Be(0.5);
        options.MinimumThroughput.Should().Be(10);
        options.BreakDurationSeconds.Should().Be(30);
    }

    [Fact]
    public void TimeoutOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var options = new TimeoutOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.TimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void ResilienceOptions_ShouldContainAllSubOptions()
    {
        // Arrange & Act
        var options = new ResilienceOptions();

        // Assert
        options.Retry.Should().NotBeNull();
        options.CircuitBreaker.Should().NotBeNull();
        options.Timeout.Should().NotBeNull();
    }
}
