using Dawning.Identity.Application.Interfaces.Security;
using Dawning.Identity.Application.Services.Security;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Dawning.Identity.Application.Tests.Services;

public class LoginLockoutServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly IConfiguration _configuration;
    private readonly LoginLockoutService _lockoutService;

    public LoginLockoutServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _uowMock.Setup(x => x.User).Returns(_userRepositoryMock.Object);

        // Use real configuration instead of Mock (extension methods cannot be mocked)
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Security:Lockout:Enabled"] = "true",
                ["Security:Lockout:MaxFailedAttempts"] = "5",
                ["Security:Lockout:LockoutDurationMinutes"] = "30",
            }
        );
        _configuration = configBuilder.Build();

        _lockoutService = new LoginLockoutService(_uowMock.Object, _configuration);
    }

    [Fact]
    public async Task IsLockedOutAsync_Should_Return_Null_When_Not_Locked()
    {
        // Arrange
        var username = "testuser";
        _userRepositoryMock
            .Setup(x => x.GetLockoutEndAsync(username))
            .ReturnsAsync((DateTime?)null);

        // Act
        var result = await _lockoutService.IsLockedOutAsync(username);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsLockedOutAsync_Should_Return_LockoutEnd_When_Locked()
    {
        // Arrange
        var username = "testuser";
        var lockoutEnd = DateTime.UtcNow.AddMinutes(30);
        _userRepositoryMock.Setup(x => x.GetLockoutEndAsync(username)).ReturnsAsync(lockoutEnd);

        // Act
        var result = await _lockoutService.IsLockedOutAsync(username);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeCloseTo(lockoutEnd, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RecordFailedLoginAsync_Should_Record_Failed_Attempt()
    {
        // Arrange
        var username = "testuser";
        _userRepositoryMock
            .Setup(x => x.RecordFailedLoginAsync(username, 5, 30))
            .ReturnsAsync((3, false, null as DateTime?));

        // Act
        var result = await _lockoutService.RecordFailedLoginAsync(username);

        // Assert
        result.failedCount.Should().Be(3);
        result.isLockedOut.Should().BeFalse();
        result.lockoutEnd.Should().BeNull();
    }

    [Fact]
    public async Task RecordFailedLoginAsync_Should_Lockout_After_Max_Attempts()
    {
        // Arrange
        var username = "testuser";
        var lockoutEnd = DateTime.UtcNow.AddMinutes(30);
        _userRepositoryMock
            .Setup(x => x.RecordFailedLoginAsync(username, 5, 30))
            .ReturnsAsync((5, true, lockoutEnd));

        // Act
        var result = await _lockoutService.RecordFailedLoginAsync(username);

        // Assert
        result.failedCount.Should().Be(5);
        result.isLockedOut.Should().BeTrue();
        result.lockoutEnd.Should().NotBeNull();
    }

    [Fact]
    public async Task ResetFailedCountAsync_Should_Reset_Count()
    {
        // Arrange
        var username = "testuser";
        _userRepositoryMock
            .Setup(x => x.ResetFailedLoginCountAsync(username))
            .Returns(Task.CompletedTask);

        // Act
        await _lockoutService.ResetFailedCountAsync(username);

        // Assert
        _userRepositoryMock.Verify(x => x.ResetFailedLoginCountAsync(username), Times.Once);
    }

    [Fact]
    public async Task UnlockUserAsync_Should_Unlock_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.UnlockUserAsync(userId)).Returns(Task.CompletedTask);

        // Act
        await _lockoutService.UnlockUserAsync(userId);

        // Assert
        _userRepositoryMock.Verify(x => x.UnlockUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetLockoutSettingsAsync_Should_Return_Settings()
    {
        // Act
        var result = await _lockoutService.GetLockoutSettingsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Enabled.Should().BeTrue();
        result.MaxFailedAttempts.Should().Be(5);
        result.LockoutDurationMinutes.Should().Be(30);
    }

    [Fact]
    public async Task IsLockedOutAsync_Should_Return_Null_When_Lockout_Disabled()
    {
        // Arrange - Create configuration with lockout disabled
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Security:Lockout:Enabled"] = "false",
                ["Security:Lockout:MaxFailedAttempts"] = "5",
                ["Security:Lockout:LockoutDurationMinutes"] = "30",
            }
        );
        var disabledConfig = configBuilder.Build();

        var service = new LoginLockoutService(_uowMock.Object, disabledConfig);

        // Act
        var result = await service.IsLockedOutAsync("testuser");

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(x => x.GetLockoutEndAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RecordFailedLoginAsync_Should_Not_Record_When_Disabled()
    {
        // Arrange - Create configuration with lockout disabled
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Security:Lockout:Enabled"] = "false",
                ["Security:Lockout:MaxFailedAttempts"] = "5",
                ["Security:Lockout:LockoutDurationMinutes"] = "30",
            }
        );
        var disabledConfig = configBuilder.Build();

        var service = new LoginLockoutService(_uowMock.Object, disabledConfig);

        // Act
        var result = await service.RecordFailedLoginAsync("testuser");

        // Assert
        result.failedCount.Should().Be(0);
        result.isLockedOut.Should().BeFalse();
        _userRepositoryMock.Verify(
            x => x.RecordFailedLoginAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never
        );
    }
}
