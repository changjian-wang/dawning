using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// 认证控制器集成测试
/// </summary>
public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Token_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", "invalid@test.com" },
            { "password", "wrongpassword" },
            { "client_id", "dawning-admin" },
            { "scope", "openid profile" }
        });

        // Act
        var response = await Client.PostAsync("/connect/token", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Token_WithMissingGrantType_ReturnsBadRequest()
    {
        // Arrange
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "username", "test@test.com" },
            { "password", "password" }
        });

        // Act
        var response = await Client.PostAsync("/connect/token", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Token_WithInvalidGrantType_ReturnsBadRequest()
    {
        // Arrange
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "invalid_grant" },
            { "username", "test@test.com" },
            { "password", "password" }
        });

        // Act
        var response = await Client.PostAsync("/connect/token", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithoutToken_ShouldNotFail()
    {
        // Act
        var response = await Client.PostAsync("/connect/logout", null);

        // Assert
        // Logout 端点应该处理未认证的请求
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}
