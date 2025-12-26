using System.Text.Json.Serialization;

namespace Dawning.Core.Results;

/// <summary>
/// 分页结果
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// 总记录数
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页码 (从1开始)
    /// </summary>
    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 总页数
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// 是否有上一页
    /// </summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// 是否有下一页
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
    /// 创建空的分页结果
    /// </summary>
    public static PagedResult<T> Empty(int pageIndex = 1, int pageSize = 10)
    {
        return new PagedResult<T>(Array.Empty<T>(), 0, pageIndex, pageSize);
    }

    /// <summary>
    /// 从列表创建分页结果 (内存分页)
    /// </summary>
    public static PagedResult<T> FromList(IEnumerable<T> source, int pageIndex, int pageSize)
    {
        var list = source.ToList();
        var items = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<T>(items, list.Count, pageIndex, pageSize);
    }

    /// <summary>
    /// 映射到另一个类型
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
/// 分页请求参数
/// </summary>
public class PagedRequest
{
    private int _pageIndex = 1;
    private int _pageSize = 10;

    /// <summary>
    /// 页码 (从1开始, 最小为1)
    /// </summary>
    [JsonPropertyName("pageIndex")]
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = Math.Max(1, value);
    }

    /// <summary>
    /// 每页大小 (1-100)
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, 100);
    }

    /// <summary>
    /// 排序字段
    /// </summary>
    [JsonPropertyName("sortField")]
    public string? SortField { get; set; }

    /// <summary>
    /// 是否降序
    /// </summary>
    [JsonPropertyName("sortDescending")]
    public bool SortDescending { get; set; }

    /// <summary>
    /// 搜索关键字
    /// </summary>
    [JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    /// <summary>
    /// 计算跳过的记录数
    /// </summary>
    [JsonIgnore]
    public int Skip => (PageIndex - 1) * PageSize;

    /// <summary>
    /// 获取记录数 (等于 PageSize)
    /// </summary>
    [JsonIgnore]
    public int Take => PageSize;
}
