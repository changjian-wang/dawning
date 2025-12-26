using System.Security.Claims;
using Dawning.Identity;
using Dawning.Identity.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Dawning.Identity.Tests;

public class CurrentUserTests
{
    [Fact]
    public void CurrentUser_WithValidClaims_ShouldExtractUserInfo()
    {
        // Arrange - 使用 DawningClaimTypes 定义的 claim type
        var claims = new List<Claim>
        {
            new Claim(DawningClaimTypes.UserId, "123"),
            new Claim(DawningClaimTypes.UserName, "testuser"),
            new Claim(DawningClaimTypes.Email, "test@example.com"),
            new Claim(DawningClaimTypes.Role, "Admin"),
            new Claim(DawningClaimTypes.Role, "User"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(principal);

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        // Act
        var currentUser = new CurrentUser(mockAccessor.Object);

        // Assert
        currentUser.UserId.Should().Be("123");
        currentUser.UserName.Should().Be("testuser");
        currentUser.Email.Should().Be("test@example.com");
        currentUser.Roles.Should().Contain("Admin");
        currentUser.Roles.Should().Contain("User");
        currentUser.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void CurrentUser_WithoutAuthentication_ShouldReturnDefaults()
    {
        // Arrange
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var currentUser = new CurrentUser(mockAccessor.Object);

        // Assert
        currentUser.IsAuthenticated.Should().BeFalse();
        currentUser.UserId.Should().BeNull();
    }

    [Fact]
    public void HasRole_WhenUserHasRole_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(principal);

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        var currentUser = new CurrentUser(mockAccessor.Object);

        // Act & Assert
        currentUser.HasRole("Admin").Should().BeTrue();
        currentUser.HasRole("User").Should().BeFalse();
    }
}
