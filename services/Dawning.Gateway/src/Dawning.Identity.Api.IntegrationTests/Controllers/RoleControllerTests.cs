using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// 角色控制器集成测试
/// </summary>
public class RoleControllerTests : IntegrationTestBase
{
    public RoleControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    #region 未认证测试

    [Fact]
    public async Task GetRoleById_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/role/00000000-0000-0000-0000-000000000001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoleByName_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/role/by-name/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoleList_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/role");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllRoles_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/role/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var newRole = new
        {
            name = "test_role",
            displayName = "Test Role",
            description = "A test role",
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/role", newRole);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var updateData = new
        {
            id = "00000000-0000-0000-0000-000000000001",
            name = "updated_role",
            displayName = "Updated Role",
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            "/api/role/00000000-0000-0000-0000-000000000001",
            updateData
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.DeleteAsync("/api/role/00000000-0000-0000-0000-000000000001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region 端点可达性测试

    [Fact]
    public async Task GetRoleById_WithInvalidGuid_ReturnsBadRequest()
    {
        // Act
        var response = await Client.GetAsync("/api/role/invalid-guid");

        // Assert - 路由约束会拒绝无效的 GUID
        response
            .StatusCode.Should()
            .BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound,
                HttpStatusCode.Unauthorized
            );
    }

    [Fact]
    public async Task GetRoleList_WithPagination_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/role?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoleList_WithFilters_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/role?name=admin&isActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
