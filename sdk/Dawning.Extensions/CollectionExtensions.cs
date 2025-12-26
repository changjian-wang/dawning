namespace Dawning.Extensions;

/// <summary>
/// 集合扩展方法
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// 检查集合是否为 null 或空
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) =>
        source == null || !source.Any();

    /// <summary>
    /// 如果集合为 null，返回空集合
    /// </summary>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source) =>
        source ?? Enumerable.Empty<T>();

    /// <summary>
    /// 对集合中的每个元素执行操作
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// 对集合中的每个元素执行操作（带索引）
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        var index = 0;
        foreach (var item in source)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// 将集合分成指定大小的批次
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                "Batch size must be greater than 0"
            );

        return source
            .Select((item, index) => new { item, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.item));
    }

    /// <summary>
    /// 获取集合中的不同元素（基于指定的键选择器）
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var seenKeys = new HashSet<TKey>();
        foreach (var item in source)
        {
            if (seenKeys.Add(keySelector(item)))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// 安全地获取字典值，如果键不存在则返回默认值
    /// </summary>
    public static TValue? GetValueOrDefault<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue? defaultValue = default
    )
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// 将集合转换为只读集合
    /// </summary>
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.ToList().AsReadOnly();
    }

    /// <summary>
    /// 将集合打乱顺序
    /// </summary>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.OrderBy(_ => Random.Shared.Next());
    }

    /// <summary>
    /// 从集合中随机获取一个元素
    /// </summary>
    public static T? RandomElement<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var list = source.ToList();
        return list.Count == 0 ? default : list[Random.Shared.Next(list.Count)];
    }

    /// <summary>
    /// 合并两个字典，后者的值优先
    /// </summary>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        this IDictionary<TKey, TValue> first,
        IDictionary<TKey, TValue> second
    )
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var result = new Dictionary<TKey, TValue>(first);
        foreach (var kvp in second)
        {
            result[kvp.Key] = kvp.Value;
        }
        return result;
    }

    /// <summary>
    /// 将集合连接成字符串
    /// </summary>
    public static string JoinToString<T>(this IEnumerable<T> source, string separator = ", ")
    {
        ArgumentNullException.ThrowIfNull(source);
        return string.Join(separator, source);
    }

    /// <summary>
    /// 添加元素到集合（如果不存在）
    /// </summary>
    public static bool AddIfNotExists<T>(this ICollection<T> collection, T item)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (collection.Contains(item))
            return false;

        collection.Add(item);
        return true;
    }

    /// <summary>
    /// 添加多个元素到集合
    /// </summary>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
