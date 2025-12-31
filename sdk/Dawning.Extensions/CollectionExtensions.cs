namespace Dawning.Extensions;

/// <summary>
/// Collection extension methods
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Checks if the collection is null or empty
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) =>
        source == null || !source.Any();

    /// <summary>
    /// Returns an empty collection if the source is null
    /// </summary>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source) =>
        source ?? Enumerable.Empty<T>();

    /// <summary>
    /// Executes an action on each element in the collection
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
    /// Executes an action on each element in the collection (with index)
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
    /// Splits the collection into batches of the specified size
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
    /// Gets distinct elements from the collection based on a key selector
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
    /// Safely gets a dictionary value, returns default if key doesn't exist
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
    /// Converts the collection to a read-only collection
    /// </summary>
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.ToList().AsReadOnly();
    }

    /// <summary>
    /// Shuffles the collection randomly
    /// </summary>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.OrderBy(_ => Random.Shared.Next());
    }

    /// <summary>
    /// Gets a random element from the collection
    /// </summary>
    public static T? RandomElement<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var list = source.ToList();
        return list.Count == 0 ? default : list[Random.Shared.Next(list.Count)];
    }

    /// <summary>
    /// Merges two dictionaries, with the second dictionary's values taking precedence
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
    /// Joins the collection into a string
    /// </summary>
    public static string JoinToString<T>(this IEnumerable<T> source, string separator = ", ")
    {
        ArgumentNullException.ThrowIfNull(source);
        return string.Join(separator, source);
    }

    /// <summary>
    /// Adds an element to the collection if it doesn't already exist
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
    /// Adds multiple elements to the collection
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
