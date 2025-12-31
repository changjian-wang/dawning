using System.ComponentModel;
using System.Reflection;

namespace Dawning.Extensions;

/// <summary>
/// Object extension methods
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Checks if the object is null
    /// </summary>
    public static bool IsNull<T>(this T? obj)
        where T : class => obj == null;

    /// <summary>
    /// Checks if the object is not null
    /// </summary>
    public static bool IsNotNull<T>(this T? obj)
        where T : class => obj != null;

    /// <summary>
    /// Returns the default value if the object is null
    /// </summary>
    public static T IfNull<T>(this T? obj, T defaultValue)
        where T : class => obj ?? defaultValue;

    /// <summary>
    /// Executes a factory method to get the default value if the object is null
    /// </summary>
    public static T IfNull<T>(this T? obj, Func<T> defaultFactory)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(defaultFactory);
        return obj ?? defaultFactory();
    }

    /// <summary>
    /// Executes an action and returns the object if the condition is true
    /// </summary>
    public static T When<T>(this T obj, bool condition, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (condition)
            action(obj);
        return obj;
    }

    /// <summary>
    /// Executes a transformation and returns the result if the condition is true
    /// </summary>
    public static T When<T>(this T obj, bool condition, Func<T, T> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        return condition ? transform(obj) : obj;
    }

    /// <summary>
    /// Converts the object to a dictionary
    /// </summary>
    public static Dictionary<string, object?> ToDictionary(this object? obj)
    {
        if (obj == null)
            return new Dictionary<string, object?>();

        return obj.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p.GetValue(obj));
    }

    /// <summary>
    /// Checks if the object is in the specified list of values
    /// </summary>
    public static bool IsIn<T>(this T obj, params T[] values) => values.Contains(obj);

    /// <summary>
    /// Checks if the object is not in the specified list of values
    /// </summary>
    public static bool IsNotIn<T>(this T obj, params T[] values) => !values.Contains(obj);

    /// <summary>
    /// Executes an action and returns the object (for chaining)
    /// </summary>
    public static T Tap<T>(this T obj, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action(obj);
        return obj;
    }

    /// <summary>
    /// Transforms the object
    /// </summary>
    public static TResult Pipe<T, TResult>(this T obj, Func<T, TResult> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        return transform(obj);
    }

    /// <summary>
    /// Attempts to cast the object to the specified type
    /// </summary>
    public static T? As<T>(this object? obj)
        where T : class => obj as T;

    /// <summary>
    /// Gets the Description attribute value of an enum
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Safely gets a property value
    /// </summary>
    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        if (string.IsNullOrWhiteSpace(propertyName))
            return null;

        var property = obj.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(obj);
    }

    /// <summary>
    /// Safely sets a property value
    /// </summary>
    public static bool SetPropertyValue(this object obj, string propertyName, object? value)
    {
        ArgumentNullException.ThrowIfNull(obj);
        if (string.IsNullOrWhiteSpace(propertyName))
            return false;

        var property = obj.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null || !property.CanWrite)
            return false;

        property.SetValue(obj, value);
        return true;
    }

    /// <summary>
    /// Clamps the value within the specified range
    /// </summary>
    public static T Clamp<T>(this T value, T min, T max)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;
        return value;
    }

    /// <summary>
    /// Checks if the value is within the specified range
    /// </summary>
    public static bool IsBetween<T>(this T value, T min, T max, bool inclusive = true)
        where T : IComparable<T>
    {
        if (inclusive)
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        return value.CompareTo(min) > 0 && value.CompareTo(max) < 0;
    }
}
