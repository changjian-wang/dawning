using AutoMapper;
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

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _uowMock.Setup(x => x.Role).Returns(_roleRepositoryMock.Object);

        _roleService = new RoleService(_uowMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Role_When_Exists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            Name = "admin",
            DisplayName = "Administrator",
            Description = "Admin role",
            IsActive = true,
            IsSystem = true,
        };

        var roleDto = new RoleDto
        {
            Id = roleId,
            Name = "admin",
            DisplayName = "Administrator",
            Description = "Admin role",
            IsActive = true,
            IsSystem = true,
        };

        _roleRepositoryMock.Setup(x => x.GetAsync(roleId)).ReturnsAsync(role);
        _mapperMock.Setup(x => x.Map<RoleDto>(role)).Returns(roleDto);

        // Act
        var result = await _roleService.GetAsync(roleId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("admin");
        result.DisplayName.Should().Be("Administrator");
        _roleRepositoryMock.Verify(x => x.GetAsync(roleId), Times.Once);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleRepositoryMock.Setup(x => x.GetAsync(roleId)).ReturnsAsync((Role?)null);

        // Act
        var result = await _roleService.GetAsync(roleId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_Should_Return_Role()
    {
        // Arrange
        var roleName = "admin";
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            DisplayName = "Administrator",
        };

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = roleName,
            DisplayName = "Administrator",
        };

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(roleName)).ReturnsAsync(role);
        _mapperMock.Setup(x => x.Map<RoleDto>(role)).Returns(roleDto);

        // Act
        var result = await _roleService.GetByNameAsync(roleName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(roleName);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_Role()
    {
        // Arrange
        var createDto = new CreateRoleDto
        {
            Name = "newrole",
            DisplayName = "New Role",
            Description = "A new role",
            IsActive = true,
        };

        var roleDto = new RoleDto
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            DisplayName = createDto.DisplayName,
            Description = createDto.Description,
            IsActive = true,
        };

        _roleRepositoryMock.Setup(x => x.NameExistsAsync(createDto.Name, null)).ReturnsAsync(false);
        _roleRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<Role>())).ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<Role>(createDto)).Returns(new Role { Name = createDto.Name });
        _mapperMock.Setup(x => x.Map<RoleDto>(It.IsAny<Role>())).Returns(roleDto);

        // Act
        var result = await _roleService.CreateAsync(createDto, null);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.DisplayName.Should().Be(createDto.DisplayName);
        _roleRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Role>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Name_Exists()
    {
        // Arrange
        var createDto = new CreateRoleDto { Name = "existingrole" };

        _roleRepositoryMock.Setup(x => x.NameExistsAsync(createDto.Name, null)).ReturnsAsync(true);

        // Act & Assert
        var action = async () => await _roleService.CreateAsync(createDto, null);
        await action
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Role()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var updateDto = new UpdateRoleDto
        {
            Id = roleId,
            DisplayName = "Updated Role",
            Description = "Updated description",
            IsActive = false,
        };

        var existingRole = new Role
        {
            Id = roleId,
            Name = "testrole",
            DisplayName = "Test Role",
            IsSystem = false,
            IsActive = true,
        };

        var updatedRoleDto = new RoleDto
        {
            Id = roleId,
            Name = "testrole",
            DisplayName = updateDto.DisplayName,
            Description = updateDto.Description,
            IsActive = false,
        };

        _roleRepositoryMock.Setup(x => x.GetAsync(roleId)).ReturnsAsync(existingRole);
        _roleRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Role>())).ReturnsAsync(true);
        _mapperMock.Setup(x => x.Map<RoleDto>(It.IsAny<Role>())).Returns(updatedRoleDto);

        // Act
        var result = await _roleService.UpdateAsync(updateDto, null);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be(updateDto.DisplayName);
        _roleRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Role>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Role_Not_Found()
    {
        // Arrange
        var updateDto = new UpdateRoleDto { Id = Guid.NewGuid() };
        _roleRepositoryMock.Setup(x => x.GetAsync(updateDto.Id)).ReturnsAsync((Role?)null);

        // Act & Assert
        var action = async () => await _roleService.UpdateAsync(updateDto, null);
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_System_Role()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var updateDto = new UpdateRoleDto { Id = roleId };
        var systemRole = new Role { Id = roleId, IsSystem = true };

        _roleRepositoryMock.Setup(x => x.GetAsync(roleId)).ReturnsAsync(systemRole);

        // Act & Assert
        var action = async () => await _roleService.UpdateAsync(updateDto, null);
        await action
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*System roles cannot be modified*");
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Roles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "admin",
                DisplayName = "Admin",
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "user",
                DisplayName = "User",
            },
        };

        var roleDtos = new List<RoleDto>
        {
            new()
            {
                Id = roles[0].Id,
                Name = "admin",
                DisplayName = "Admin",
            },
            new()
            {
                Id = roles[1].Id,
                Name = "user",
                DisplayName = "User",
            },
        };

        _roleRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(roles);
        _mapperMock.Setup(x => x.Map<RoleDto>(roles[0])).Returns(roleDtos[0]);
        _mapperMock.Setup(x => x.Map<RoleDto>(roles[1])).Returns(roleDtos[1]);

        // Act
        var result = await _roleService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_Paged_Roles()
    {
        // Arrange
        var model = new RoleModel();
        var roles = new List<Role>
        {
            new() { Id = Guid.NewGuid(), Name = "role1" },
            new() { Id = Guid.NewGuid(), Name = "role2" },
        };

        var pagedData = new PagedData<Role>
        {
            PageIndex = 1,
            PageSize = 10,
            TotalCount = 2,
            Items = roles,
        };

        _roleRepositoryMock.Setup(x => x.GetPagedListAsync(model, 1, 10)).ReturnsAsync(pagedData);
        _mapperMock.Setup(x => x.Map<RoleDto>(It.IsAny<Role>())).Returns(new RoleDto());

        // Act
        var result = await _roleService.GetPagedListAsync(model, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.PageIndex.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(2);
    }
}
