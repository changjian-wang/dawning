using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// 用户控制器集成测试
/// </summary>
public class UserControllerTests : IntegrationTestBase
{
    public UserControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCurrentUserInfo_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/user/info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/user/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var newUser = new
        {
            username = "testuser",
            email = "test@test.com",
            password = "Test@123456"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/user", newUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUser_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var updateData = new { email = "updated@test.com" };

        // Act
        var response = await Client.PutAsJsonAsync("/api/user/1", updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUser_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.DeleteAsync("/api/user/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var passwordData = new
        {
            currentPassword = "oldpassword",
            newPassword = "newpassword"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/user/change-password", passwordData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserRoles_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/user/1/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
