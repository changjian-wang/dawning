using Dawning.Identity.Application.Dtos.Monitoring;
using Dawning.Identity.Application.Interfaces.Monitoring;
using Dawning.Identity.Application.Services.Monitoring;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Interfaces.Monitoring;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Monitoring;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dawning.Identity.Application.Tests.Services;

public class AlertServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAlertRuleRepository> _alertRuleRepoMock;
    private readonly Mock<IAlertHistoryRepository> _alertHistoryRepoMock;
    private readonly Mock<IAlertNotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<AlertService>> _loggerMock;
    private readonly IAlertService _alertService;

    public AlertServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _alertRuleRepoMock = new Mock<IAlertRuleRepository>();
        _alertHistoryRepoMock = new Mock<IAlertHistoryRepository>();
        _notificationServiceMock = new Mock<IAlertNotificationService>();
        _loggerMock = new Mock<ILogger<AlertService>>();

        _unitOfWorkMock.Setup(x => x.AlertRule).Returns(_alertRuleRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.AlertHistory).Returns(_alertHistoryRepoMock.Object);

        _alertService = new AlertService(
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object
        );
    }

    #region 告警规则测试

    [Fact]
    public async Task GetAllRulesAsync_Should_Return_All_Rules()
    {
        // Arrange
        var rules = new List<AlertRule>
        {
            new() { Id = 1, Name = "Rule 1", MetricType = "cpu", IsEnabled = true },
            new() { Id = 2, Name = "Rule 2", MetricType = "memory", IsEnabled = false }
        };
        _alertRuleRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(rules);

        // Act
        var result = await _alertService.GetAllRulesAsync();

        // Assert
        result.Should().HaveCount(2);
        _alertRuleRepoMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEnabledRulesAsync_Should_Return_Only_Enabled_Rules()
    {
        // Arrange
        var rules = new List<AlertRule>
        {
            new() { Id = 1, Name = "Rule 1", MetricType = "cpu", IsEnabled = true }
        };
        _alertRuleRepoMock.Setup(x => x.GetEnabledAsync()).ReturnsAsync(rules);

        // Act
        var result = await _alertService.GetEnabledRulesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetRuleByIdAsync_Should_Return_Rule_When_Exists()
    {
        // Arrange
        var rule = new AlertRule
        {
            Id = 1,
            Name = "Test Rule",
            MetricType = "cpu",
            Operator = "gt",
            Threshold = 80,
            Severity = "warning"
        };
        _alertRuleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(rule);

        // Act
        var result = await _alertService.GetRuleByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Rule");
        result.MetricType.Should().Be("cpu");
    }

    [Fact]
    public async Task GetRuleByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        _alertRuleRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((AlertRule?)null);

        // Act
        var result = await _alertService.GetRuleByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateRuleAsync_Should_Create_And_Return_Rule()
    {
        // Arrange
        var request = new CreateAlertRuleRequest
        {
            Name = "New Rule",
            MetricType = "cpu",
            Operator = "gt",
            Threshold = 90,
            Severity = "critical",
            IsEnabled = true,
            DurationSeconds = 60,
            CooldownMinutes = 5
        };

        _alertRuleRepoMock.Setup(x => x.CreateAsync(It.IsAny<AlertRule>()))
            .ReturnsAsync(1L);

        // Act
        var result = await _alertService.CreateRuleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Rule");
        result.MetricType.Should().Be("cpu");
        _alertRuleRepoMock.Verify(x => x.CreateAsync(It.IsAny<AlertRule>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRuleAsync_Should_Return_Null_When_Rule_Not_Found()
    {
        // Arrange
        _alertRuleRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((AlertRule?)null);

        var request = new UpdateAlertRuleRequest
        {
            Name = "Updated Rule",
            MetricType = "memory"
        };

        // Act
        var result = await _alertService.UpdateRuleAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRuleAsync_Should_Update_And_Return_Rule()
    {
        // Arrange
        var existingRule = new AlertRule
        {
            Id = 1,
            Name = "Old Rule",
            MetricType = "cpu",
            Operator = "gt",
            Threshold = 80,
            Severity = "warning"
        };

        var updatedRule = new AlertRule
        {
            Id = 1,
            Name = "Updated Rule",
            MetricType = "memory",
            Operator = "gt",
            Threshold = 90,
            Severity = "critical"
        };

        _alertRuleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingRule);
        _alertRuleRepoMock.Setup(x => x.UpdateAsync(It.IsAny<AlertRule>())).ReturnsAsync(true);
        _alertRuleRepoMock.SetupSequence(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingRule)
            .ReturnsAsync(updatedRule);

        var request = new UpdateAlertRuleRequest
        {
            Name = "Updated Rule",
            MetricType = "memory",
            Operator = "gt",
            Threshold = 90,
            Severity = "critical"
        };

        // Act
        var result = await _alertService.UpdateRuleAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        _alertRuleRepoMock.Verify(x => x.UpdateAsync(It.IsAny<AlertRule>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRuleAsync_Should_Call_Repository()
    {
        // Arrange
        _alertRuleRepoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _alertService.DeleteRuleAsync(1);

        // Assert
        result.Should().BeTrue();
        _alertRuleRepoMock.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task SetRuleEnabledAsync_Should_Toggle_Rule_Status()
    {
        // Arrange
        _alertRuleRepoMock.Setup(x => x.SetEnabledAsync(1, true)).ReturnsAsync(true);

        // Act
        var result = await _alertService.SetRuleEnabledAsync(1, true);

        // Assert
        result.Should().BeTrue();
        _alertRuleRepoMock.Verify(x => x.SetEnabledAsync(1, true), Times.Once);
    }

    #endregion

    #region 告警历史测试

    [Fact]
    public async Task GetAlertHistoryAsync_Should_Return_Paged_Results()
    {
        // Arrange
        var histories = new List<AlertHistory>
        {
            new() { Id = 1, RuleName = "Rule 1", Status = "triggered" },
            new() { Id = 2, RuleName = "Rule 2", Status = "resolved" }
        };

        var pagedResult = new PagedData<AlertHistory>
        {
            Items = histories,
            TotalCount = 2,
            PageIndex = 1,
            PageSize = 10
        };

        _alertHistoryRepoMock.Setup(x => x.GetPagedListAsync(
            It.IsAny<AlertHistoryQueryModel>(), 1, 10))
            .ReturnsAsync(pagedResult);

        var queryParams = new AlertHistoryQueryParams { Page = 1, PageSize = 10 };

        // Act
        var result = await _alertService.GetAlertHistoryAsync(queryParams);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAlertHistoryByIdAsync_Should_Return_History_When_Exists()
    {
        // Arrange
        var history = new AlertHistory
        {
            Id = 1,
            RuleId = 1,
            RuleName = "Test Rule",
            Status = "triggered",
            Severity = "warning"
        };
        _alertHistoryRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(history);

        // Act
        var result = await _alertService.GetAlertHistoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.RuleName.Should().Be("Test Rule");
    }

    [Fact]
    public async Task GetAlertHistoryByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        _alertHistoryRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((AlertHistory?)null);

        // Act
        var result = await _alertService.GetAlertHistoryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAlertStatusAsync_Should_Acknowledge_When_Status_Is_Acknowledged()
    {
        // Arrange
        var request = new UpdateAlertStatusRequest
        {
            Status = "acknowledged",
            ResolvedBy = "admin"
        };
        _alertHistoryRepoMock.Setup(x => x.AcknowledgeAsync(1, "admin")).ReturnsAsync(true);

        // Act
        var result = await _alertService.UpdateAlertStatusAsync(1, request);

        // Assert
        result.Should().BeTrue();
        _alertHistoryRepoMock.Verify(x => x.AcknowledgeAsync(1, "admin"), Times.Once);
    }

    [Fact]
    public async Task UpdateAlertStatusAsync_Should_Resolve_When_Status_Is_Resolved()
    {
        // Arrange
        var request = new UpdateAlertStatusRequest
        {
            Status = "resolved",
            ResolvedBy = "admin"
        };
        _alertHistoryRepoMock.Setup(x => x.ResolveAsync(1, "admin")).ReturnsAsync(true);

        // Act
        var result = await _alertService.UpdateAlertStatusAsync(1, request);

        // Assert
        result.Should().BeTrue();
        _alertHistoryRepoMock.Verify(x => x.ResolveAsync(1, "admin"), Times.Once);
    }

    [Fact]
    public async Task GetUnresolvedAlertsAsync_Should_Return_Unresolved_Alerts()
    {
        // Arrange
        var histories = new List<AlertHistory>
        {
            new() { Id = 1, RuleName = "Rule 1", Status = "triggered" },
            new() { Id = 2, RuleName = "Rule 2", Status = "acknowledged" }
        };
        _alertHistoryRepoMock.Setup(x => x.GetUnresolvedAsync()).ReturnsAsync(histories);

        // Act
        var result = await _alertService.GetUnresolvedAlertsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(x => x.Status != "resolved").Should().BeTrue();
    }

    #endregion

    #region 统计测试

    [Fact]
    public async Task GetAlertStatisticsAsync_Should_Return_Statistics()
    {
        // Arrange
        var statistics = new AlertStatistics
        {
            TotalRules = 10,
            EnabledRules = 8,
            TotalAlertsToday = 5,
            UnresolvedAlerts = 2,
            CriticalAlerts = 1,
            WarningAlerts = 3
        };

        _alertHistoryRepoMock.Setup(x => x.GetStatisticsAsync()).ReturnsAsync(statistics);

        // Act
        var result = await _alertService.GetAlertStatisticsAsync();

        // Assert
        result.TotalRules.Should().Be(10);
        result.EnabledRules.Should().Be(8);
        result.TotalAlertsToday.Should().Be(5);
        result.UnresolvedAlerts.Should().Be(2);
    }

    #endregion
}
