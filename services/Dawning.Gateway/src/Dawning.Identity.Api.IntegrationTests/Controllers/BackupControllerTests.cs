using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// 备份控制器集成测试
/// 备份控制器端点: POST /api/backup, GET /api/backup/history, DELETE /api/backup/{id}, POST /api/backup/cleanup
/// </summary>
public class BackupControllerTests : IntegrationTestBase
{
    public BackupControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    #region 未认证测试

    [Fact]
    public async Task GetBackupHistory_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/backup/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateBackup_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.PostAsync("/api/backup", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteBackup_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.DeleteAsync("/api/backup/00000000-0000-0000-0000-000000000001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CleanupBackups_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.PostAsync("/api/backup/cleanup", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region 端点存在性测试

    [Fact]
    public async Task BackupHistoryEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/backup/history");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBackupEndpoint_Exists()
    {
        // Act
        var response = await Client.PostAsync("/api/backup", null);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CleanupEndpoint_Exists()
    {
        // Act
        var response = await Client.PostAsync("/api/backup/cleanup", null);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    #endregion
}
