using AutoMapper;
using Dawning.Identity.Application.Dtos.User;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Events;
using Dawning.Identity.Application.Services.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dawning.Identity.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IIntegrationEventBus> _integrationEventBusMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _integrationEventBusMock = new Mock<IIntegrationEventBus>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _uowMock.Object,
            _mapperMock.Object,
            _integrationEventBusMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_User_When_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            DisplayName = "Test User",
            Role = "user",
            IsActive = true,
        };

        var userDto = new UserDto
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            DisplayName = "Test User",
            Role = "user",
            IsActive = true,
        };

        _userRepositoryMock.Setup(x => x.GetAsync(userId)).ReturnsAsync(user);

        _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.Email.Should().Be("test@test.com");
        _userRepositoryMock.Verify(x => x.GetAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_Return_User()
    {
        // Arrange
        var username = "testuser";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = "test@test.com",
        };

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = username,
            Email = "test@test.com",
        };

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(username)).ReturnsAsync(user);

        _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _userService.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_User()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Username = "newuser",
            Password = "Password123!",
            Email = "new@test.com",
            DisplayName = "New User",
            Role = "user",
            IsActive = true,
        };

        var userId = Guid.NewGuid();
        var userDto = new UserDto
        {
            Id = userId,
            Username = createDto.Username,
            Email = createDto.Email,
            DisplayName = createDto.DisplayName,
            Role = createDto.Role,
            IsActive = true,
        };

        _userRepositoryMock
            .Setup(x => x.UsernameExistsAsync(createDto.Username, null))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(createDto.Email!, null))
            .ReturnsAsync(false);

        _userRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<User>())).ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>())).Returns(userDto);

        // Act
        var result = await _userService.CreateAsync(createDto, null);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(createDto.Username);
        result.Email.Should().Be(createDto.Email);
        _userRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto
        {
            Id = userId,
            Email = "updated@test.com",
            DisplayName = "Updated User",
            Role = "admin",
            IsActive = false,
        };

        var existingUser = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "old@test.com",
            DisplayName = "Old User",
            Role = "user",
            IsActive = true,
        };

        var updatedUserDto = new UserDto
        {
            Id = userId,
            Username = "testuser",
            Email = updateDto.Email,
            DisplayName = updateDto.DisplayName,
            Role = updateDto.Role,
            IsActive = false,
        };

        _userRepositoryMock.Setup(x => x.GetAsync(userId)).ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(updateDto.Email!, userId))
            .ReturnsAsync(false);

        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);

        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>())).Returns(updatedUserDto);

        // Act
        var result = await _userService.UpdateAsync(updateDto, null);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(updateDto.Email);
        result.DisplayName.Should().Be(updateDto.DisplayName);
        result.Role.Should().Be(updateDto.Role);
        result.IsActive.Should().Be(updateDto.IsActive!.Value);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
        };

        _userRepositoryMock.Setup(x => x.GetAsync(userId)).ReturnsAsync(user);

        _userRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<User>())).ReturnsAsync(true);

        // Act
        await _userService.DeleteAsync(userId);

        // Assert
        _userRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_Paged_Users()
    {
        // Arrange
        var model = new UserModel { Username = "test" };
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Username = "testuser1",
                Email = "test1@test.com",
            },
            new()
            {
                Id = Guid.NewGuid(),
                Username = "testuser2",
                Email = "test2@test.com",
            },
        };

        var pagedData = new PagedData<User>
        {
            PageIndex = 1,
            PageSize = 10,
            TotalCount = 2,
            Items = users,
        };

        var userDtos = new List<UserDto>
        {
            new()
            {
                Id = users[0].Id,
                Username = "testuser1",
                Email = "test1@test.com",
            },
            new()
            {
                Id = users[1].Id,
                Username = "testuser2",
                Email = "test2@test.com",
            },
        };

        _userRepositoryMock.Setup(x => x.GetPagedListAsync(model, 1, 10)).ReturnsAsync(pagedData);

        _mapperMock.Setup(x => x.Map<UserDto>(users[0])).Returns(userDtos[0]);

        _mapperMock.Setup(x => x.Map<UserDto>(users[1])).Returns(userDtos[1]);

        // Act
        var result = await _userService.GetPagedListAsync(model, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.PageIndex.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ResetPasswordAsync_Should_Reset_User_Password()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newPassword = "NewPassword123!";
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "oldHash",
        };

        _userRepositoryMock.Setup(x => x.GetAsync(userId)).ReturnsAsync(user);

        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);

        // Act
        await _userService.ResetPasswordAsync(userId, newPassword);

        // Assert
        _userRepositoryMock.Verify(x => x.GetAsync(userId), Times.Once);
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<User>(u => u.Id == userId && u.PasswordHash != "oldHash")),
            Times.Once
        );
    }
}
