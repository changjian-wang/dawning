using Dawning.Logging;
using FluentAssertions;

namespace Dawning.Logging.Tests;

public class DawningLogOptionsTests
{
    [Fact]
    public void DawningLogOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var options = new DawningLogOptions();

        // Assert
        options.ApplicationName.Should().Be("DawningApp");
        options.EnableConsole.Should().BeTrue();
        options.EnableFile.Should().BeTrue();
    }

    [Fact]
    public void DawningLogOptions_ShouldAllowCustomization()
    {
        // Arrange & Act
        var options = new DawningLogOptions
        {
            ApplicationName = "TestApp",
            EnableConsole = false,
            EnableFile = true,
            LogFilePath = "logs/test-.log",
        };

        // Assert
        options.ApplicationName.Should().Be("TestApp");
        options.EnableConsole.Should().BeFalse();
        options.EnableFile.Should().BeTrue();
        options.LogFilePath.Should().Be("logs/test-.log");
    }

    [Fact]
    public void MinimumLevel_ShouldDefaultToInformation()
    {
        // Arrange & Act
        var options = new DawningLogOptions();

        // Assert
        options.MinimumLevel.Should().Be(LogLevel.Information);
    }
}
