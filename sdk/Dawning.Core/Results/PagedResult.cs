using System.Text.Json.Serialization;

namespace Dawning.Core.Results;

/// <summary>
/// Paged result
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Data list
    /// </summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Total record count
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page index (starting from 1)
    /// </summary>
    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Total pages count
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Has previous page
    /// </summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Has next page
    /// </summary>
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => PageIndex < TotalPages;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageIndex, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    /// <summary>
    /// Create an empty paged result
    /// </summary>
    public static PagedResult<T> Empty(int pageIndex = 1, int pageSize = 10)
    {
        return new PagedResult<T>(Array.Empty<T>(), 0, pageIndex, pageSize);
    }

    /// <summary>
    /// Create paged result from list (in-memory paging)
    /// </summary>
    public static PagedResult<T> FromList(IEnumerable<T> source, int pageIndex, int pageSize)
    {
        var list = source.ToList();
        var items = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<T>(items, list.Count, pageIndex, pageSize);
    }

    /// <summary>
    /// Map to another type
    /// </summary>
    public PagedResult<TDestination> Map<TDestination>(Func<T, TDestination> mapper)
    {
        return new PagedResult<TDestination>
        {
            Items = Items.Select(mapper).ToList(),
            TotalCount = TotalCount,
            PageIndex = PageIndex,
            PageSize = PageSize,
        };
    }
}

/// <summary>
/// Paged request parameters
/// </summary>
public class PagedRequest
{
    private int _pageIndex = 1;
    private int _pageSize = 10;

    /// <summary>
    /// Page index (starting from 1, minimum is 1)
    /// </summary>
    [JsonPropertyName("pageIndex")]
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = Math.Max(1, value);
    }

    /// <summary>
    /// Page size (1-100)
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, 100);
    }

    /// <summary>
    /// Sort field
    /// </summary>
    [JsonPropertyName("sortField")]
    public string? SortField { get; set; }

    /// <summary>
    /// Sort in descending order
    /// </summary>
    [JsonPropertyName("sortDescending")]
    public bool SortDescending { get; set; }

    /// <summary>
    /// Search keyword
    /// </summary>
    [JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    /// <summary>
    /// Number of records to skip
    /// </summary>
    [JsonIgnore]
    public int Skip => (PageIndex - 1) * PageSize;

    /// <summary>
    /// Number of records to take (equals PageSize)
    /// </summary>
    [JsonIgnore]
    public int Take => PageSize;
}
