using System.Security.Claims;
using Dawning.Identity.Constants;
using Dawning.Identity.Extensions;
using FluentAssertions;

namespace Dawning.Identity.Tests;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ShouldReturnUserId()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(DawningClaimTypes.UserId, "user-123") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var userId = principal.GetUserId();

        // Assert
        userId.Should().Be("user-123");
    }

    [Fact]
    public void GetUserId_WhenNoClaim_ShouldReturnNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var userId = principal.GetUserId();

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void HasRole_WhenUserHasRole_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Admin") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var hasRole = principal.HasRole("Admin");

        // Assert
        hasRole.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WhenUserDoesNotHaveRole_ShouldReturnFalse()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, "User") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var hasRole = principal.HasRole("Admin");

        // Assert
        hasRole.Should().BeFalse();
    }

    [Fact]
    public void GetRoles_ShouldReturnAllRoles()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User"),
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var roles = principal.GetRoles().ToList();

        // Assert
        roles.Should().HaveCount(2);
        roles.Should().Contain("Admin");
        roles.Should().Contain("User");
    }
}
