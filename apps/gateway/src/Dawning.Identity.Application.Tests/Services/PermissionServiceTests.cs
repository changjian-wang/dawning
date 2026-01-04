using Dawning.Identity.Application.Dtos.Administration;
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

public class PermissionServiceTests
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly PermissionService _permissionService;

    public PermissionServiceTests()
    {
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _uowMock.Setup(x => x.Permission).Returns(_permissionRepositoryMock.Object);
        _uowMock.Setup(x => x.AuditLog).Returns(_auditLogRepositoryMock.Object);

        _permissionService = new PermissionService(_uowMock.Object);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Permission_When_Exists()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var permission = new Permission
        {
            Id = permissionId,
            Code = "user:read",
            Name = "Read Users",
            Resource = "user",
            Action = "read",
            IsActive = true,
        };

        _permissionRepositoryMock.Setup(x => x.GetAsync(permissionId)).ReturnsAsync(permission);

        // Act
        var result = await _permissionService.GetAsync(permissionId);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("user:read");
        result.Name.Should().Be("Read Users");
        _permissionRepositoryMock.Verify(x => x.GetAsync(permissionId), Times.Once);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        _permissionRepositoryMock
            .Setup(x => x.GetAsync(permissionId))
            .ReturnsAsync((Permission?)null);

        // Act
        var result = await _permissionService.GetAsync(permissionId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_Should_Return_Permission()
    {
        // Arrange
        var code = "user:read";
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = "Read Users",
        };

        _permissionRepositoryMock.Setup(x => x.GetByCodeAsync(code)).ReturnsAsync(permission);

        // Act
        var result = await _permissionService.GetByCodeAsync(code);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be(code);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_Permission()
    {
        // Arrange
        var createDto = new CreatePermissionDto
        {
            Code = "user:create",
            Name = "Create Users",
            Description = "Permission to create users",
            Resource = "user",
            Action = "create",
            Category = "User Management",
            IsActive = true,
            DisplayOrder = 1,
        };

        _permissionRepositoryMock
            .Setup(x => x.CodeExistsAsync(createDto.Code, null))
            .ReturnsAsync(false);
        _permissionRepositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Permission>()))
            .ReturnsAsync(true);
        _auditLogRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<AuditLog>())).ReturnsAsync(1);

        // Act
        var result = await _permissionService.CreateAsync(createDto, null);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be(createDto.Code);
        result.Name.Should().Be(createDto.Name);
        _permissionRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Permission>()), Times.Once);
        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Code_Exists()
    {
        // Arrange
        var createDto = new CreatePermissionDto { Code = "existing:code" };

        _permissionRepositoryMock
            .Setup(x => x.CodeExistsAsync(createDto.Code, null))
            .ReturnsAsync(true);

        // Act & Assert
        var action = async () => await _permissionService.CreateAsync(createDto, null);
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Permissions()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = "user:read",
                Name = "Read Users",
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "user:write",
                Name = "Write Users",
            },
        };

        _permissionRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(permissions);

        // Act
        var result = await _permissionService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByRoleIdAsync_Should_Return_Role_Permissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissions = new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Code = "user:read" },
        };

        _permissionRepositoryMock.Setup(x => x.GetByRoleIdAsync(roleId)).ReturnsAsync(permissions);

        // Act
        var result = await _permissionService.GetByRoleIdAsync(roleId);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetGroupedPermissionsAsync_Should_Return_Grouped_Permissions()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = "user:read",
                Resource = "user",
                DisplayOrder = 1,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "user:write",
                Resource = "user",
                DisplayOrder = 2,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "role:read",
                Resource = "role",
                DisplayOrder = 1,
            },
        };

        _permissionRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(permissions);

        // Act
        var result = await _permissionService.GetGroupedPermissionsAsync();

        // Assert
        result.Should().HaveCount(2);
        var groups = result.ToList();
        groups.First(g => g.Resource == "user").Permissions.Should().HaveCount(2);
        groups.First(g => g.Resource == "role").Permissions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_Paged_Permissions()
    {
        // Arrange
        var model = new PermissionModel();
        var permissions = new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Code = "p1" },
            new() { Id = Guid.NewGuid(), Code = "p2" },
        };

        var pagedData = new PagedData<Permission>
        {
            PageIndex = 1,
            PageSize = 10,
            TotalCount = 2,
            Items = permissions,
        };

        _permissionRepositoryMock
            .Setup(x => x.GetPagedListAsync(model, 1, 10))
            .ReturnsAsync(pagedData);

        // Act
        var result = await _permissionService.GetPagedListAsync(model, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.PageIndex.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }
}
