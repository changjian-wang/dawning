using FluentAssertions;
using Xunit;

namespace Dawning.Extensions.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("hello", false)]
    public void IsNullOrWhiteSpace_ShouldReturnCorrectResult(string? input, bool expected)
    {
        input.IsNullOrWhiteSpace().Should().Be(expected);
    }

    [Theory]
    [InlineData("", "default", "default")]
    [InlineData(null, "default", "default")]
    [InlineData("hello", "default", "hello")]
    public void IfNullOrWhiteSpace_ShouldReturnCorrectValue(string? input, string defaultValue, string expected)
    {
        input.IfNullOrWhiteSpace(defaultValue).Should().Be(expected);
    }

    [Theory]
    [InlineData("hello world", 5, "he...")]
    [InlineData("hi", 10, "hi")]
    [InlineData(null, 5, "")]
    public void Truncate_ShouldTruncateCorrectly(string? input, int maxLength, string expected)
    {
        input.Truncate(maxLength).Should().Be(expected);
    }

    [Theory]
    [InlineData("hello_world", "HelloWorld")]
    [InlineData("hello-world", "HelloWorld")]
    [InlineData("hello world", "HelloWorld")]
    public void ToPascalCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToPascalCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "helloWorld")]
    [InlineData("hello_world", "helloWorld")]
    public void ToCamelCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToCamelCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("helloWorld", "hello_world")]
    public void ToSnakeCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToSnakeCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("helloWorld", "hello-world")]
    public void ToKebabCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToKebabCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    public void IsValidEmail_ShouldValidateCorrectly(string input, bool expected)
    {
        input.IsValidEmail().Should().Be(expected);
    }

    [Theory]
    [InlineData("13812345678", true)]
    [InlineData("12345678901", false)]
    [InlineData("1381234567", false)]
    public void IsValidPhoneNumber_ShouldValidateCorrectly(string input, bool expected)
    {
        input.IsValidPhoneNumber().Should().Be(expected);
    }

    [Fact]
    public void Mask_ShouldMaskCorrectly()
    {
        "13812345678".Mask().Should().Be("138****5678");
        "abc".Mask().Should().Be("***");
    }

    [Theory]
    [InlineData("hello world", 0, 5, "hello")]
    [InlineData("hello", 10, null, "")]
    [InlineData(null, 0, 5, "")]
    public void SafeSubstring_ShouldReturnSafeResult(string? input, int start, int? length, string expected)
    {
        input.SafeSubstring(start, length).Should().Be(expected);
    }

    [Theory]
    [InlineData("hello world", "helloworld")]
    [InlineData("  a  b  ", "ab")]
    [InlineData(null, "")]
    public void RemoveWhitespace_ShouldRemoveAllWhitespace(string? input, string expected)
    {
        input.RemoveWhitespace().Should().Be(expected);
    }
}
