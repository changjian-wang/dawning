using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// 审计日志控制器集成测试
/// 审计日志端点: GET /api/audit-log, GET /api/audit-log/{id}
/// </summary>
public class AuditLogControllerTests : IntegrationTestBase
{
    public AuditLogControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region 未认证测试

    [Fact]
    public async Task GetAuditLogs_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/audit-log");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuditLogById_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/audit-log/00000000-0000-0000-0000-000000000001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region 端点存在性测试

    [Fact]
    public async Task AuditLogEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/audit-log");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AuditLogList_WithPagination_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/audit-log?page=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuditLogList_WithFilters_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/audit-log?entityType=User&action=Create");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuditLogById_Endpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/audit-log/00000000-0000-0000-0000-000000000001");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    #endregion
}
