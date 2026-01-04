using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// Permission controller integration tests
/// </summary>
public class PermissionControllerTests : IntegrationTestBase
{
    public PermissionControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    #region Unauthenticated tests

    [Fact]
    public async Task GetPermissionById_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(
            "/api/permission/00000000-0000-0000-0000-000000000001"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPermissionByCode_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/permission/code/user.read");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPermissionList_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/permission");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllPermissions_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/permission/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGroupedPermissions_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/permission/grouped");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPermissionsByRoleId_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(
            "/api/permission/role/00000000-0000-0000-0000-000000000001"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePermission_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var newPermission = new
        {
            code = "test.permission",
            name = "Test Permission",
            resource = "test",
            action = "read",
            description = "A test permission",
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/permission", newPermission);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePermission_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var updateData = new
        {
            id = "00000000-0000-0000-0000-000000000001",
            code = "updated.permission",
            name = "Updated Permission",
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            "/api/permission/00000000-0000-0000-0000-000000000001",
            updateData
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePermission_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.DeleteAsync(
            "/api/permission/00000000-0000-0000-0000-000000000001"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AssignPermissionsToRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var permissionIds = new[] { "00000000-0000-0000-0000-000000000001" };

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/permission/role/00000000-0000-0000-0000-000000000001/assign",
            permissionIds
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Endpoint reachability tests

    [Fact]
    public async Task GetPermissionList_WithPagination_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/permission?page=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPermissionList_WithFilters_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(
            "/api/permission?code=user&resource=user&isActive=true"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
