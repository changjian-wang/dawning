using Dawning.Core.Results;
using FluentAssertions;

namespace Dawning.Core.Tests;

public class ApiResultTests
{
    [Fact]
    public void Ok_WithData_ShouldReturnSuccessResult()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var result = ApiResults.Ok(data);

        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().Be("OK");
        result.Data.Should().Be(data);
    }

    [Fact]
    public void Ok_WithMessage_ShouldReturnCustomMessage()
    {
        // Arrange
        var message = "操作成功";

        // Act
        var result = ApiResults.Ok("data", message);

        // Assert
        result.Message.Should().Be(message);
    }

    [Fact]
    public void Fail_ShouldReturnFailureResult()
    {
        // Arrange
        var code = "ERROR";
        var errorMessage = "操作失败";

        // Act
        var result = ApiResults.Fail(code, errorMessage);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void Fail_WithCode_ShouldReturnCorrectCode()
    {
        // Arrange
        var code = "VALIDATION_ERROR";
        var message = "参数错误";

        // Act
        var result = ApiResults.Fail(code, message);

        // Assert
        result.Code.Should().Be(code);
    }

    [Fact]
    public void ApiResult_ShouldHaveTimestamp()
    {
        // Arrange & Act
        var result = ApiResults.Ok("test");

        // Assert
        result.Timestamp.Should().BePositive();
    }
}
