using Dawning.Resilience.Options;
using Dawning.Resilience.Policies;
using FluentAssertions;

namespace Dawning.Resilience.Tests;

public class ResiliencePolicyBuilderTests
{
    [Fact]
    public void Build_WithDefaultOptions_ShouldCreatePipeline()
    {
        // Arrange
        var options = new ResilienceOptions();
        var builder = new ResiliencePolicyBuilder(options);

        // Act
        var pipeline = builder.Build();

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithDisabledRetry_ShouldNotIncludeRetry()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            Retry = { Enabled = false },
            CircuitBreaker = { Enabled = false },
            Timeout = { Enabled = false }
        };
        var builder = new ResiliencePolicyBuilder(options);

        // Act
        var pipeline = builder.Build();

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task Build_GenericPipeline_ShouldExecuteSuccessfully()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            Retry = { Enabled = false },
            CircuitBreaker = { Enabled = false },
            Timeout = { Enabled = false }
        };
        var builder = new ResiliencePolicyBuilder(options);
        var pipeline = builder.Build<string>();

        // Act
        var result = await pipeline.ExecuteAsync(async token =>
        {
            await Task.Delay(1);
            return "success";
        });

        // Assert
        result.Should().Be("success");
    }
}
