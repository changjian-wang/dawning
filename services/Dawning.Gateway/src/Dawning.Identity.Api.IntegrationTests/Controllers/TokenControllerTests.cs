using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// Token 控制器集成测试
/// Token控制器端点: GET /api/token/sessions, DELETE /api/token/sessions/{deviceId},
/// POST /api/token/sessions/revoke-others, POST /api/token/revoke-all
/// </summary>
public class TokenControllerTests : IntegrationTestBase
{
    public TokenControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    #region 未认证测试

    [Fact]
    public async Task GetSessions_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/token/sessions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RevokeSession_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.DeleteAsync("/api/token/sessions/device-123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RevokeOtherSessions_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.PostAsync("/api/token/sessions/revoke-others", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RevokeAllTokens_WithoutAuth_ReturnsUnauthorized()
    {
        // Act - 正确的端点是 /api/token/revoke-all
        var response = await Client.PostAsync("/api/token/revoke-all", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLoginPolicy_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/token/policy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region 端点存在性测试

    [Fact]
    public async Task SessionsEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/token/sessions");

        // Assert - 应返回 Unauthorized 而不是 NotFound
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RevokeSessionEndpoint_Exists()
    {
        // Act
        var response = await Client.DeleteAsync("/api/token/sessions/test-device");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PolicyEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/token/policy");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    #endregion
}
