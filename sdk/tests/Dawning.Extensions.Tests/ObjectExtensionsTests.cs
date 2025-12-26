using System.ComponentModel;
using FluentAssertions;
using Xunit;

namespace Dawning.Extensions.Tests;

public class ObjectExtensionsTests
{
    private class TestClass
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    private enum TestEnum
    {
        [Description("First Value")]
        First,
        Second,
    }

    [Fact]
    public void IsNull_ShouldReturnTrueForNull()
    {
        TestClass? obj = null;
        obj.IsNull().Should().BeTrue();
    }

    [Fact]
    public void IsNull_ShouldReturnFalseForNonNull()
    {
        var obj = new TestClass();
        obj.IsNull().Should().BeFalse();
    }

    [Fact]
    public void IsNotNull_ShouldReturnFalseForNull()
    {
        TestClass? obj = null;
        obj.IsNotNull().Should().BeFalse();
    }

    [Fact]
    public void IfNull_ShouldReturnDefaultValueForNull()
    {
        TestClass? obj = null;
        var defaultObj = new TestClass { Name = "Default" };

        obj.IfNull(defaultObj).Should().BeSameAs(defaultObj);
    }

    [Fact]
    public void IfNull_ShouldReturnOriginalForNonNull()
    {
        var obj = new TestClass { Name = "Original" };
        var defaultObj = new TestClass { Name = "Default" };

        obj.IfNull(defaultObj).Should().BeSameAs(obj);
    }

    [Fact]
    public void IfNull_WithFactory_ShouldCallFactoryOnlyForNull()
    {
        var factoryCalled = false;
        TestClass? obj = null;

        obj.IfNull(() =>
        {
            factoryCalled = true;
            return new TestClass();
        });

        factoryCalled.Should().BeTrue();
    }

    [Fact]
    public void When_ShouldExecuteActionWhenConditionIsTrue()
    {
        var obj = new TestClass { Name = "Test" };
        var actionExecuted = false;

        obj.When(true, x => actionExecuted = true);

        actionExecuted.Should().BeTrue();
    }

    [Fact]
    public void When_ShouldNotExecuteActionWhenConditionIsFalse()
    {
        var obj = new TestClass { Name = "Test" };
        var actionExecuted = false;

        obj.When(false, x => actionExecuted = true);

        actionExecuted.Should().BeFalse();
    }

    [Fact]
    public void ToDictionary_ShouldConvertObjectToDictionary()
    {
        var obj = new TestClass { Name = "Test", Age = 25 };
        var dict = obj.ToDictionary();

        dict.Should().ContainKey("Name");
        dict.Should().ContainKey("Age");
        dict["Name"].Should().Be("Test");
        dict["Age"].Should().Be(25);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(5, false)]
    public void IsIn_ShouldCheckIfValueIsInList(int value, bool expected)
    {
        value.IsIn(1, 2, 3).Should().Be(expected);
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(2, false)]
    public void IsNotIn_ShouldCheckIfValueIsNotInList(int value, bool expected)
    {
        value.IsNotIn(1, 2, 3).Should().Be(expected);
    }

    [Fact]
    public void Tap_ShouldExecuteActionAndReturnObject()
    {
        var obj = new TestClass { Name = "Test" };
        var tapped = false;

        var result = obj.Tap(x => tapped = true);

        tapped.Should().BeTrue();
        result.Should().BeSameAs(obj);
    }

    [Fact]
    public void Pipe_ShouldTransformObject()
    {
        var obj = new TestClass { Name = "Test", Age = 25 };
        var result = obj.Pipe(x => x.Name + "-" + x.Age);

        result.Should().Be("Test-25");
    }

    [Fact]
    public void As_ShouldCastObject()
    {
        object obj = new TestClass { Name = "Test" };
        var result = obj.As<TestClass>();

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public void As_ShouldReturnNullForInvalidCast()
    {
        object obj = "string";
        var result = obj.As<TestClass>();

        result.Should().BeNull();
    }

    [Fact]
    public void GetDescription_ShouldReturnDescriptionAttribute()
    {
        TestEnum.First.GetDescription().Should().Be("First Value");
    }

    [Fact]
    public void GetDescription_ShouldReturnEnumNameIfNoAttribute()
    {
        TestEnum.Second.GetDescription().Should().Be("Second");
    }

    [Fact]
    public void GetPropertyValue_ShouldReturnPropertyValue()
    {
        var obj = new TestClass { Name = "Test" };
        obj.GetPropertyValue("Name").Should().Be("Test");
    }

    [Fact]
    public void SetPropertyValue_ShouldSetPropertyValue()
    {
        var obj = new TestClass { Name = "Original" };
        var result = obj.SetPropertyValue("Name", "New");

        result.Should().BeTrue();
        obj.Name.Should().Be("New");
    }

    [Theory]
    [InlineData(5, 0, 10, 5)]
    [InlineData(-5, 0, 10, 0)]
    [InlineData(15, 0, 10, 10)]
    public void Clamp_ShouldClampValue(int value, int min, int max, int expected)
    {
        value.Clamp(min, max).Should().Be(expected);
    }

    [Theory]
    [InlineData(5, 0, 10, true, true)]
    [InlineData(0, 0, 10, true, true)]
    [InlineData(0, 0, 10, false, false)]
    [InlineData(-1, 0, 10, true, false)]
    public void IsBetween_ShouldCheckRange(
        int value,
        int min,
        int max,
        bool inclusive,
        bool expected
    )
    {
        value.IsBetween(min, max, inclusive).Should().Be(expected);
    }
}
