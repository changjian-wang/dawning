using Dawning.Identity.Application.Services.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dawning.Identity.Application.Tests.Services;

/// <summary>
/// 系统配置服务测试（不使用缓存）
/// </summary>
public class SystemConfigServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISystemConfigRepository> _configRepoMock;
    private readonly ISystemConfigService _configService;

    public SystemConfigServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _configRepoMock = new Mock<ISystemConfigRepository>();

        _unitOfWorkMock.Setup(x => x.SystemConfig).Returns(_configRepoMock.Object);

        // 不使用缓存的配置服务
        _configService = new SystemConfigService(_unitOfWorkMock.Object, null);
    }

    [Fact]
    public async Task GetValueAsync_Should_Return_Value_When_Exists()
    {
        // Arrange
        var config = new SystemConfigAggregate
        {
            Id = Guid.NewGuid(),
            Name = "System",
            Key = "SiteName",
            Value = "Dawning Gateway",
        };

        var pagedResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate> { config },
            TotalCount = 1,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _configService.GetValueAsync("System", "SiteName");

        // Assert
        result.Should().Be("Dawning Gateway");
    }

    [Fact]
    public async Task GetValueAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        var emptyResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate>(),
            TotalCount = 0,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _configService.GetValueAsync("System", "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValueAsync_With_Default_Should_Return_Default_When_Not_Exists()
    {
        // Arrange
        var emptyResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate>(),
            TotalCount = 0,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _configService.GetValueAsync("System", "NonExistent", "DefaultValue");

        // Assert
        result.Should().Be("DefaultValue");
    }

    [Fact]
    public async Task GetValueAsync_Generic_Should_Parse_Integer()
    {
        // Arrange
        var config = new SystemConfigAggregate
        {
            Id = Guid.NewGuid(),
            Name = "System",
            Key = "MaxItems",
            Value = "100",
        };

        var pagedResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate> { config },
            TotalCount = 1,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _configService.GetValueAsync("System", "MaxItems", 50);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public async Task GetValueAsync_Generic_Should_Parse_Boolean()
    {
        // Arrange
        var config = new SystemConfigAggregate
        {
            Id = Guid.NewGuid(),
            Name = "System",
            Key = "EnableFeature",
            Value = "true",
        };

        var pagedResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate> { config },
            TotalCount = 1,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _configService.GetValueAsync("System", "EnableFeature", false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetValueAsync_Should_Create_New_Config_When_Not_Exists()
    {
        // Arrange
        var emptyResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate>(),
            TotalCount = 0,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(emptyResult);

        _configRepoMock
            .Setup(x => x.InsertAsync(It.IsAny<SystemConfigAggregate>()))
            .ReturnsAsync(1);

        // Act
        var result = await _configService.SetValueAsync(
            "System",
            "NewKey",
            "NewValue",
            "Description"
        );

        // Assert
        result.Should().BeTrue();
        _configRepoMock.Verify(x => x.InsertAsync(It.IsAny<SystemConfigAggregate>()), Times.Once);
    }

    [Fact]
    public async Task SetValueAsync_Should_Update_Existing_Config()
    {
        // Arrange
        var existingConfig = new SystemConfigAggregate
        {
            Id = Guid.NewGuid(),
            Name = "System",
            Key = "ExistingKey",
            Value = "OldValue",
        };

        var existingResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate> { existingConfig },
            TotalCount = 1,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(existingResult);

        _configRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<SystemConfigAggregate>()))
            .ReturnsAsync(true);

        // Act
        var result = await _configService.SetValueAsync("System", "ExistingKey", "NewValue");

        // Assert
        result.Should().BeTrue();
        _configRepoMock.Verify(x => x.UpdateAsync(It.IsAny<SystemConfigAggregate>()), Times.Once);
    }

    [Fact]
    public async Task GetByGroupAsync_Should_Return_Configs_In_Group()
    {
        // Arrange
        var configs = new List<SystemConfigAggregate>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "System",
                Key = "Key1",
                Value = "Value1",
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "System",
                Key = "Key2",
                Value = "Value2",
            },
        };

        var pagedResult = new PagedData<SystemConfigAggregate>
        {
            Items = configs,
            TotalCount = 2,
            PageIndex = 1,
            PageSize = 1000,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1000))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _configService.GetByGroupAsync("System");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetGroupsAsync_Should_Return_All_Distinct_Groups()
    {
        // Arrange
        var configs = new List<SystemConfigAggregate>
        {
            new() { Name = "System", Key = "Key1" },
            new() { Name = "System", Key = "Key2" },
            new() { Name = "Security", Key = "Key1" },
            new() { Name = "Email", Key = "Key1" },
        };

        var pagedResult = new PagedData<SystemConfigAggregate>
        {
            Items = configs,
            TotalCount = 4,
            PageIndex = 1,
            PageSize = 10000,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 10000))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _configService.GetGroupsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(new[] { "Email", "Security", "System" });
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Config()
    {
        // Arrange
        var config = new SystemConfigAggregate
        {
            Id = Guid.NewGuid(),
            Name = "System",
            Key = "ToDelete",
        };

        var existingResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate> { config },
            TotalCount = 1,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(existingResult);

        _configRepoMock
            .Setup(x => x.DeleteAsync(It.IsAny<SystemConfigAggregate>()))
            .ReturnsAsync(true);

        // Act
        var result = await _configService.DeleteAsync("System", "ToDelete");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_False_When_Not_Exists()
    {
        // Arrange
        var emptyResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate>(),
            TotalCount = 0,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _configService.DeleteAsync("System", "NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task BatchUpdateAsync_Should_Update_Multiple_Configs()
    {
        // Arrange
        var configs = new List<SystemConfigItemDto>
        {
            new()
            {
                Group = "System",
                Key = "Key1",
                Value = "Value1",
            },
            new()
            {
                Group = "System",
                Key = "Key2",
                Value = "Value2",
            },
        };

        var emptyResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate>(),
            TotalCount = 0,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(emptyResult);

        _configRepoMock
            .Setup(x => x.InsertAsync(It.IsAny<SystemConfigAggregate>()))
            .ReturnsAsync(1);

        // Act
        var result = await _configService.BatchUpdateAsync(configs);

        // Assert
        result.Should().BeTrue();
        _configRepoMock.Verify(
            x => x.InsertAsync(It.IsAny<SystemConfigAggregate>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task GetLastUpdateTimestampAsync_Should_Return_Latest_Timestamp()
    {
        // Arrange
        var expectedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var config = new SystemConfigAggregate
        {
            Id = Guid.NewGuid(),
            Name = "System",
            Key = "Key1",
            Timestamp = expectedTimestamp,
        };

        var pagedResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate> { config },
            TotalCount = 1,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _configService.GetLastUpdateTimestampAsync();

        // Assert
        result.Should().Be(expectedTimestamp);
    }

    [Fact]
    public async Task GetLastUpdateTimestampAsync_Should_Return_Zero_When_No_Configs()
    {
        // Arrange
        var emptyResult = new PagedData<SystemConfigAggregate>
        {
            Items = new List<SystemConfigAggregate>(),
            TotalCount = 0,
            PageIndex = 1,
            PageSize = 1,
        };

        _configRepoMock
            .Setup(x => x.GetPagedListAsync(It.IsAny<SystemConfigModel>(), 1, 1))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _configService.GetLastUpdateTimestampAsync();

        // Assert
        result.Should().Be(0);
    }
}
