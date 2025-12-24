using FluentAssertions;
using Xunit;

namespace Dawning.Extensions.Tests;

public class JsonExtensionsTests
{
    private class TestObject
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    [Fact]
    public void ToJson_ShouldSerializeObject()
    {
        var obj = new TestObject { Name = "Test", Age = 25 };
        var json = obj.ToJson();

        json.Should().Contain("\"name\"");
        json.Should().Contain("\"age\"");
    }

    [Fact]
    public void ToJson_WithIndent_ShouldBeFormatted()
    {
        var obj = new TestObject { Name = "Test", Age = 25 };
        var json = obj.ToJson(indented: true);

        json.Should().Contain("\n");
    }

    [Fact]
    public void FromJson_ShouldDeserializeObject()
    {
        var json = "{\"name\":\"Test\",\"age\":25}";
        var obj = json.FromJson<TestObject>();

        obj.Should().NotBeNull();
        obj!.Name.Should().Be("Test");
        obj.Age.Should().Be(25);
    }

    [Fact]
    public void TryFromJson_ShouldReturnTrueForValidJson()
    {
        var json = "{\"name\":\"Test\"}";
        var result = json.TryFromJson<TestObject>(out var obj);

        result.Should().BeTrue();
        obj.Should().NotBeNull();
    }

    [Fact]
    public void TryFromJson_ShouldReturnFalseForInvalidJson()
    {
        var json = "invalid json";
        var result = json.TryFromJson<TestObject>(out var obj);

        result.Should().BeFalse();
        obj.Should().BeNull();
    }

    [Fact]
    public void DeepClone_ShouldCreateIndependentCopy()
    {
        var original = new TestObject { Name = "Original", Age = 30 };
        var clone = original.DeepClone();

        clone.Should().NotBeNull();
        clone!.Name.Should().Be("Original");

        // Modify original
        original.Name = "Modified";
        clone.Name.Should().Be("Original");
    }

    [Theory]
    [InlineData("{}", true)]
    [InlineData("{\"key\":\"value\"}", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidJson_ShouldValidateCorrectly(string? json, bool expected)
    {
        json.IsValidJson().Should().Be(expected);
    }

    [Fact]
    public void PrettyPrint_ShouldFormatJson()
    {
        var json = "{\"name\":\"Test\",\"age\":25}";
        var pretty = json.PrettyPrint();

        pretty.Should().Contain("\n");
    }

    [Fact]
    public void Minify_ShouldCompressJson()
    {
        var json = "{\n  \"name\": \"Test\"\n}";
        var minified = json.Minify();

        minified.Should().NotContain("\n");
    }

    [Fact]
    public void MergeJson_ShouldMergeTwoJsonObjects()
    {
        var base1 = "{\"a\":1,\"b\":2}";
        var override1 = "{\"b\":3,\"c\":4}";

        var result = base1.MergeJson(override1);

        result.Should().Contain("\"a\"");
        result.Should().Contain("\"c\"");
    }

    [Fact]
    public void GetJsonValue_ShouldExtractNestedValue()
    {
        var json = "{\"user\":{\"name\":\"Test\"}}";
        var name = json.GetJsonValue<string>("user.name");

        name.Should().Be("Test");
    }

    [Fact]
    public void GetJsonValue_ShouldReturnDefaultForMissingPath()
    {
        var json = "{\"user\":{\"name\":\"Test\"}}";
        var missing = json.GetJsonValue<string>("user.email");

        missing.Should().BeNull();
    }
}
