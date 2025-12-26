using System.Net;
using Dawning.Core.Exceptions;
using FluentAssertions;

namespace Dawning.Core.Tests;

public class BusinessExceptionTests
{
    [Fact]
    public void BusinessException_ShouldHaveDefaultStatusCode400()
    {
        // Act
        var exception = new BusinessException("业务错误");

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Message.Should().Be("业务错误");
    }

    [Fact]
    public void NotFoundException_ShouldHaveStatusCode404()
    {
        // Act
        var exception = new NotFoundException("资源不存在");

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.Message.Should().Be("资源不存在");
    }

    [Fact]
    public void UnauthorizedException_ShouldHaveStatusCode401()
    {
        // Act
        var exception = new UnauthorizedException();

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void ForbiddenException_ShouldHaveStatusCode403()
    {
        // Act
        var exception = new ForbiddenException();

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public void ValidationException_ShouldHaveStatusCode400()
    {
        // Act
        var exception = new ValidationException("field", "error");

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
