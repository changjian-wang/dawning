using Dawning.Identity.Application.Interfaces.Logging;
using Dawning.Identity.Application.Services.Logging;
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

public class RequestLoggingServiceTests
{
    private readonly Mock<IRequestLogRepository> _requestLogRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<RequestLoggingService>> _loggerMock;
    private readonly IRequestLoggingService _requestLoggingService;

    public RequestLoggingServiceTests()
    {
        _requestLogRepositoryMock = new Mock<IRequestLogRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RequestLoggingService>>();

        _uowMock.Setup(x => x.RequestLog).Returns(_requestLogRepositoryMock.Object);

        _requestLoggingService = new RequestLoggingService(
            _uowMock.Object,
            _loggerMock.Object
        );
    }

    #region LogRequestAsync Tests

    [Fact]
    public async Task LogRequestAsync_Should_Insert_Log_Entry()
    {
        // Arrange
        var entry = new RequestLogEntry
        {
            Id = Guid.NewGuid(),
            RequestId = "test-request-id",
            Method = "GET",
            Path = "/api/test",
            StatusCode = 200,
            ResponseTimeMs = 50,
            ClientIp = "127.0.0.1",
            RequestTime = DateTime.UtcNow
        };

        _requestLogRepositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<RequestLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _requestLoggingService.LogRequestAsync(entry);

        // Assert
        _requestLogRepositoryMock.Verify(
            x => x.InsertAsync(It.Is<RequestLog>(log =>
                log.RequestId == entry.RequestId &&
                log.Method == entry.Method &&
                log.Path == entry.Path &&
                log.StatusCode == entry.StatusCode
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task LogRequestAsync_Should_Handle_Null_RequestId()
    {
        // Arrange
        var entry = new RequestLogEntry
        {
            Id = Guid.NewGuid(),
            RequestId = null, // Null RequestId
            Method = "POST",
            Path = "/api/create",
            StatusCode = 201,
            ResponseTimeMs = 100,
            RequestTime = DateTime.UtcNow
        };

        _requestLogRepositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<RequestLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _requestLoggingService.LogRequestAsync(entry);

        // Assert
        _requestLogRepositoryMock.Verify(
            x => x.InsertAsync(It.Is<RequestLog>(log =>
                log.RequestId == string.Empty // Should be empty string, not null
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task LogRequestAsync_Should_Not_Throw_On_Repository_Exception()
    {
        // Arrange
        var entry = new RequestLogEntry
        {
            Id = Guid.NewGuid(),
            Method = "GET",
            Path = "/api/test",
            StatusCode = 200,
            ResponseTimeMs = 50,
            RequestTime = DateTime.UtcNow
        };

        _requestLogRepositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<RequestLog>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() =>
            _requestLoggingService.LogRequestAsync(entry)
        );

        exception.Should().BeNull();
    }

    #endregion

    #region GetLogsAsync Tests

    [Fact]
    public async Task GetLogsAsync_Should_Return_Paged_Results()
    {
        // Arrange
        var query = new RequestLogQuery
        {
            Page = 1,
            PageSize = 10
        };

        var logs = new List<RequestLog>
        {
            new RequestLog
            {
                Id = Guid.NewGuid(),
                RequestId = "req-1",
                Method = "GET",
                Path = "/api/test1",
                StatusCode = 200,
                ResponseTimeMs = 50,
                RequestTime = DateTime.UtcNow
            },
            new RequestLog
            {
                Id = Guid.NewGuid(),
                RequestId = "req-2",
                Method = "POST",
                Path = "/api/test2",
                StatusCode = 201,
                ResponseTimeMs = 100,
                RequestTime = DateTime.UtcNow
            }
        };

        var pagedData = new PagedData<RequestLog>
        {
            Items = logs,
            TotalCount = 2,
            PageIndex = 1,
            PageSize = 10
        };

        _requestLogRepositoryMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<RequestLogQueryModel>(), 1, 10))
            .ReturnsAsync(pagedData);

        // Act
        var result = await _requestLoggingService.GetLogsAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetLogsAsync_Should_Pass_Filter_Parameters()
    {
        // Arrange
        var query = new RequestLogQuery
        {
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow,
            Method = "GET",
            Path = "/api",
            StatusCode = 200,
            OnlyErrors = false,
            Page = 1,
            PageSize = 20
        };

        var pagedData = new PagedData<RequestLog>
        {
            Items = new List<RequestLog>(),
            TotalCount = 0,
            PageIndex = 1,
            PageSize = 20
        };

        _requestLogRepositoryMock
            .Setup(x => x.GetPagedListAsync(
                It.Is<RequestLogQueryModel>(q =>
                    q.Method == "GET" &&
                    q.Path == "/api" &&
                    q.StatusCode == 200
                ),
                1,
                20
            ))
            .ReturnsAsync(pagedData);

        // Act
        var result = await _requestLoggingService.GetLogsAsync(query);

        // Assert
        _requestLogRepositoryMock.Verify(
            x => x.GetPagedListAsync(
                It.Is<RequestLogQueryModel>(q => q.Method == "GET"),
                1,
                20
            ),
            Times.Once
        );
    }

    #endregion

    #region GetStatisticsAsync Tests

    [Fact]
    public async Task GetStatisticsAsync_Should_Return_Statistics()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(-1);
        var endTime = DateTime.UtcNow;

        var statistics = new RequestLogStatistics
        {
            TotalRequests = 100,
            SuccessRequests = 90,
            ClientErrors = 5,
            ServerErrors = 5,
            AverageResponseTimeMs = 50,
            MaxResponseTimeMs = 500,
            MinResponseTimeMs = 5,
            P95ResponseTimeMs = 200,
            P99ResponseTimeMs = 400,
            StartTime = startTime,
            EndTime = endTime,
            StatusCodeDistribution = new Dictionary<int, long>
            {
                { 200, 80 },
                { 201, 10 },
                { 400, 3 },
                { 404, 2 },
                { 500, 5 }
            },
            TopPaths = new List<PathStatisticModel>
            {
                new PathStatisticModel
                {
                    Path = "/api/user",
                    RequestCount = 50,
                    AverageResponseTimeMs = 30,
                    ErrorCount = 2
                }
            },
            HourlyRequests = new Dictionary<string, long>
            {
                { "2025-12-22 10:00", 20 },
                { "2025-12-22 11:00", 30 }
            }
        };

        _requestLogRepositoryMock
            .Setup(x => x.GetStatisticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(statistics);

        // Act
        var result = await _requestLoggingService.GetStatisticsAsync(startTime, endTime);

        // Assert
        result.Should().NotBeNull();
        result.TotalRequests.Should().Be(100);
        result.SuccessRequests.Should().Be(90);
        result.ClientErrors.Should().Be(5);
        result.ServerErrors.Should().Be(5);
        result.AverageResponseTimeMs.Should().Be(50);
        result.StatusCodeDistribution.Should().HaveCount(5);
        result.TopPaths.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_Should_Use_Default_Time_Range_When_Null()
    {
        // Arrange
        var statistics = new RequestLogStatistics
        {
            TotalRequests = 0,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow
        };

        _requestLogRepositoryMock
            .Setup(x => x.GetStatisticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(statistics);

        // Act
        var result = await _requestLoggingService.GetStatisticsAsync(null, null);

        // Assert
        result.Should().NotBeNull();
        _requestLogRepositoryMock.Verify(
            x => x.GetStatisticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Once
        );
    }

    #endregion

    #region CleanupOldLogsAsync Tests

    [Fact]
    public async Task CleanupOldLogsAsync_Should_Delete_Old_Logs()
    {
        // Arrange
        var retentionDays = 30;
        var expectedDeletedCount = 100;

        _requestLogRepositoryMock
            .Setup(x => x.CleanupOldLogsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(expectedDeletedCount);

        // Act
        var result = await _requestLoggingService.CleanupOldLogsAsync(retentionDays);

        // Assert
        result.Should().Be(expectedDeletedCount);
        _requestLogRepositoryMock.Verify(
            x => x.CleanupOldLogsAsync(It.Is<DateTime>(d =>
                d < DateTime.UtcNow.AddDays(-retentionDays + 1) // Cutoff should be roughly 30 days ago
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task CleanupOldLogsAsync_Should_Return_Zero_When_No_Old_Logs()
    {
        // Arrange
        _requestLogRepositoryMock
            .Setup(x => x.CleanupOldLogsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(0);

        // Act
        var result = await _requestLoggingService.CleanupOldLogsAsync(7);

        // Assert
        result.Should().Be(0);
    }

    #endregion
}
