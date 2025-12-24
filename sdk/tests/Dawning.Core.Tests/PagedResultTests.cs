using Dawning.Core.Results;
using FluentAssertions;

namespace Dawning.Core.Tests;

public class PagedResultTests
{
    [Fact]
    public void Constructor_ShouldSetCorrectPaginationInfo()
    {
        // Arrange
        var items = new List<string> { "a", "b", "c" };
        var total = 100;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = new PagedResult<string>(items, total, page, pageSize);

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(total);
        result.PageIndex.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.TotalPages.Should().Be(10);
    }

    [Fact]
    public void TotalPages_ShouldCalculateCorrectly_WhenNotExactDivision()
    {
        // Arrange & Act
        var result = new PagedResult<int>(new List<int> { 1, 2, 3 }, 25, 1, 10);

        // Assert
        result.TotalPages.Should().Be(3); // 25 / 10 = 2.5 -> 3
    }

    [Fact]
    public void Empty_ShouldReturnEmptyResult()
    {
        // Act
        var result = PagedResult<int>.Empty();

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void FromList_ShouldPageCorrectly()
    {
        // Arrange
        var source = Enumerable.Range(1, 25).ToList();

        // Act
        var result = PagedResult<int>.FromList(source, 2, 10);

        // Assert
        result.Items.Should().BeEquivalentTo(new[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 });
        result.TotalCount.Should().Be(25);
        result.PageIndex.Should().Be(2);
    }

    [Fact]
    public void HasPreviousPage_ShouldReturnFalse_OnFirstPage()
    {
        // Arrange & Act
        var result = new PagedResult<int>(new List<int>(), 100, 1, 10);

        // Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_ShouldReturnTrue_WhenMorePages()
    {
        // Arrange & Act
        var result = new PagedResult<int>(new List<int>(), 100, 1, 10);

        // Assert
        result.HasNextPage.Should().BeTrue();
    }
}
