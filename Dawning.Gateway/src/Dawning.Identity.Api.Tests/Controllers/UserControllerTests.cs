using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dawning.Identity.Application.Dtos.User;
using FluentAssertions;
using Xunit;

namespace Dawning.Identity.Api.Tests.Controllers;

/// <summary>
/// UserController 集成测试
/// </summary>
public class UserControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    [Fact]
    public async Task DevResetAdmin_Should_Create_Admin_User()
    {
        // Act
        var response = await _client.PostAsync("/api/user/dev-reset-admin", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DevResetAdminResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Code.Should().Be(0);
        result.Data.Should().NotBeNull();
        result.Data!.Username.Should().Be("admin");
        result.Message.Should().Contain("admin");
    }

    [Fact]
    public async Task GetPagedList_Should_Return_Users()
    {
        // Arrange - 确保有管理员用户
        await _client.PostAsync("/api/user/dev-reset-admin", null);

        // Act
        var response = await _client.GetAsync("/api/user?page=1&itemsPerPage=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Code.Should().Be(0);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUser_Should_Create_New_User()
    {
        // Arrange
        await _client.PostAsync("/api/user/dev-reset-admin", null);

        var createDto = new CreateUserDto
        {
            Username = $"testuser_{Guid.NewGuid():N}",
            Password = "Test@123",
            Email = $"test_{Guid.NewGuid():N}@test.com",
            DisplayName = "Test User",
            Role = "user",
            IsActive = true,
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateUserResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Code.Should().Be(0);
        result.Data.Should().NotBeNull();
        result.Data!.Username.Should().Be(createDto.Username);
        result.Data.Email.Should().Be(createDto.Email);
    }

    [Fact]
    public async Task GetById_Should_Return_User()
    {
        // Arrange - 创建用户
        await _client.PostAsync("/api/user/dev-reset-admin", null);

        var listResponse = await _client.GetAsync("/api/user?page=1&itemsPerPage=1");
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listResult = JsonSerializer.Deserialize<PagedResponse>(listContent, _jsonOptions);
        var userId = listResult!.Data!.Items[0].Id;

        // Act
        var response = await _client.GetAsync($"/api/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetUserResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Code.Should().Be(0);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateUser_Should_Update_User_Info()
    {
        // Arrange - 创建用户
        await _client.PostAsync("/api/user/dev-reset-admin", null);

        var listResponse = await _client.GetAsync("/api/user?page=1&itemsPerPage=1");
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listResult = JsonSerializer.Deserialize<PagedResponse>(listContent, _jsonOptions);
        var user = listResult!.Data!.Items[0];

        var updateDto = new UpdateUserDto
        {
            DisplayName = "Updated Name",
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{user.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateUserResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Code.Should().Be(0);
        result.Data.Should().NotBeNull();
        result.Data!.DisplayName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task ResetPassword_Should_Change_User_Password()
    {
        // Arrange - 创建用户
        await _client.PostAsync("/api/user/dev-reset-admin", null);

        var listResponse = await _client.GetAsync("/api/user?page=1&itemsPerPage=1");
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listResult = JsonSerializer.Deserialize<PagedResponse>(listContent, _jsonOptions);
        var userId = listResult!.Data!.Items[0].Id;

        var resetDto = new { newPassword = "NewPass@123" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/user/{userId}/reset-password",
            resetDto
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ResetPasswordResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Code.Should().Be(0);
        result.Message.Should().Contain("success");
    }

    [Fact]
    public async Task DeleteUser_Should_Remove_User()
    {
        // Arrange - 创建一个测试用户
        await _client.PostAsync("/api/user/dev-reset-admin", null);

        var createDto = new CreateUserDto
        {
            Username = $"deletetest_{Guid.NewGuid():N}",
            Password = "Test@123",
            Email = $"delete_{Guid.NewGuid():N}@test.com",
            DisplayName = "Delete Test",
            Role = "user",
            IsActive = true,
        };

        var createResponse = await _client.PostAsJsonAsync("/api/user", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<CreateUserResponse>(
            createContent,
            _jsonOptions
        );
        var userId = createResult!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 验证用户已被删除
        var getResponse = await _client.GetAsync($"/api/user/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

// Response DTOs
public class DevResetAdminResponse
{
    public int Code { get; set; }
    public UserDto? Data { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PagedResponse
{
    public int Code { get; set; }
    public PagedData? Data { get; set; }
}

public class PagedData
{
    public List<UserDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}

public class GetUserResponse
{
    public int Code { get; set; }
    public UserDto? Data { get; set; }
}

public class CreateUserResponse
{
    public int Code { get; set; }
    public UserDto? Data { get; set; }
}

public class UpdateUserResponse
{
    public int Code { get; set; }
    public UserDto? Data { get; set; }
}

public class ResetPasswordResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
}
