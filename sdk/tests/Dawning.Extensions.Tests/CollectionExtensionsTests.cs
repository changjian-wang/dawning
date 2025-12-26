using FluentAssertions;
using Xunit;

namespace Dawning.Extensions.Tests;

public class CollectionExtensionsTests
{
    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueForNullCollection()
    {
        List<int>? list = null;
        list.IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueForEmptyCollection()
    {
        var list = new List<int>();
        list.IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnFalseForNonEmptyCollection()
    {
        var list = new List<int> { 1, 2, 3 };
        list.IsNullOrEmpty().Should().BeFalse();
    }

    [Fact]
    public void OrEmpty_ShouldReturnEmptyEnumerableForNull()
    {
        IEnumerable<int>? list = null;
        list.OrEmpty().Should().BeEmpty();
    }

    [Fact]
    public void OrEmpty_ShouldReturnSameEnumerableForNonNull()
    {
        var list = new List<int> { 1, 2, 3 };
        list.OrEmpty().Should().BeEquivalentTo(list);
    }

    [Fact]
    public void ForEach_ShouldExecuteActionForEachElement()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = new List<int>();

        list.ForEach(x => result.Add(x * 2));

        result.Should().BeEquivalentTo(new[] { 2, 4, 6 });
    }

    [Fact]
    public void ForEach_WithIndex_ShouldProvideCorrectIndex()
    {
        var list = new List<string> { "a", "b", "c" };
        var result = new List<string>();

        list.ForEach((item, index) => result.Add($"{index}:{item}"));

        result.Should().BeEquivalentTo(new[] { "0:a", "1:b", "2:c" });
    }

    [Fact]
    public void Batch_ShouldSplitIntoBatches()
    {
        var list = Enumerable.Range(1, 10).ToList();
        var batches = list.Batch(3).ToList();

        batches.Should().HaveCount(4);
        batches[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
        batches[3].Should().BeEquivalentTo(new[] { 10 });
    }

    [Fact]
    public void DistinctBy_ShouldReturnDistinctElements()
    {
        var items = new[]
        {
            new { Name = "A", Value = 1 },
            new { Name = "B", Value = 2 },
            new { Name = "A", Value = 3 },
        };

        var result = items.DistinctBy(x => x.Name).ToList();

        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().BeEquivalentTo(new[] { "A", "B" });
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnValueIfExists()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
        dict.GetValueOrDefault("a", -1).Should().Be(1);
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnDefaultIfNotExists()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
        dict.GetValueOrDefault("b", -1).Should().Be(-1);
    }

    [Fact]
    public void Merge_ShouldMergeDictionaries()
    {
        var dict1 = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var dict2 = new Dictionary<string, int> { ["b"] = 3, ["c"] = 4 };

        var result = dict1.Merge(dict2);

        result
            .Should()
            .BeEquivalentTo(
                new Dictionary<string, int>
                {
                    ["a"] = 1,
                    ["b"] = 3,
                    ["c"] = 4,
                }
            );
    }

    [Fact]
    public void JoinToString_ShouldJoinElements()
    {
        var list = new List<int> { 1, 2, 3 };
        list.JoinToString(", ").Should().Be("1, 2, 3");
    }

    [Fact]
    public void AddIfNotExists_ShouldAddOnlyIfNotExists()
    {
        var list = new List<int> { 1, 2 };

        list.AddIfNotExists(2).Should().BeFalse();
        list.AddIfNotExists(3).Should().BeTrue();
        list.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void AddRange_ShouldAddMultipleElements()
    {
        var list = new List<int> { 1 };
        list.AddRange(new[] { 2, 3, 4 });
        list.Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });
    }
}
