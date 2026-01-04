using Dawning.Identity.Application.Interfaces.Security;
using Dawning.Identity.Application.Services.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Dawning.Identity.Application.Tests.Services;

public class PasswordPolicyServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly PasswordPolicyService _policyService;

    public PasswordPolicyServiceTests()
    {
        // Use real configuration instead of Mock (extension methods cannot be mocked)
        // Note: The actual service uses the configuration path Security:Password
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Security:Password:MinLength"] = "8",
                ["Security:Password:MaxLength"] = "128",
                ["Security:Password:RequireUppercase"] = "true",
                ["Security:Password:RequireLowercase"] = "true",
                ["Security:Password:RequireDigit"] = "true",
                ["Security:Password:RequireSpecialChar"] = "true",
                ["Security:Password:SpecialCharacters"] = "!@#$%^&*()",
            }
        );
        _configuration = configBuilder.Build();

        _policyService = new PasswordPolicyService(_configuration);
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Pass_Valid_Password()
    {
        // Arrange
        var password = "ValidPass@123";

        // Act
        var result = await _policyService.ValidatePasswordAsync(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Fail_Empty_Password()
    {
        // Act
        var result = await _policyService.ValidatePasswordAsync("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password cannot be empty");
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Fail_Null_Password()
    {
        // Act
        var result = await _policyService.ValidatePasswordAsync(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password cannot be empty");
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Fail_Short_Password()
    {
        // Arrange
        var password = "Ab@1";

        // Act
        var result = await _policyService.ValidatePasswordAsync(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least"));
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Fail_Without_Uppercase()
    {
        // Arrange
        var password = "validpass@123";

        // Act
        var result = await _policyService.ValidatePasswordAsync(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("uppercase"));
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Fail_Without_Lowercase()
    {
        // Arrange
        var password = "VALIDPASS@123";

        // Act
        var result = await _policyService.ValidatePasswordAsync(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("lowercase"));
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Fail_Without_Digit()
    {
        // Arrange
        var password = "ValidPass@abc";

        // Act
        var result = await _policyService.ValidatePasswordAsync(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("digit"));
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Fail_Without_Special_Char()
    {
        // Arrange
        var password = "ValidPass123";

        // Act
        var result = await _policyService.ValidatePasswordAsync(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("special character"));
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Fail_Common_Password()
    {
        // Arrange - Use a real weak password, password123 is in the weak password list
        var password = "password123";

        // Act
        var result = await _policyService.ValidatePasswordAsync(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("too simple"));
    }

    [Fact]
    public async Task GetPolicyAsync_Should_Return_Policy()
    {
        // Act
        var result = await _policyService.GetPolicyAsync();

        // Assert
        result.Should().NotBeNull();
        result.MinLength.Should().Be(8);
        result.MaxLength.Should().Be(128);
        result.RequireUppercase.Should().BeTrue();
        result.RequireLowercase.Should().BeTrue();
        result.RequireDigit.Should().BeTrue();
        result.RequireSpecialChar.Should().BeTrue();
    }

    [Theory]
    [InlineData("Secure@Pass1", true)]
    [InlineData("MyP@ssw0rd!", true)]
    [InlineData("abc", false)]
    [InlineData("12345678", false)]
    [InlineData("abcdefgh", false)]
    [InlineData("ABCDEFGH", false)]
    public async Task ValidatePasswordAsync_Various_Passwords(string password, bool expectedValid)
    {
        // Act
        var result = await _policyService.ValidatePasswordAsync(password);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}
