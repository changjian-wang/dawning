using System.ComponentModel;
using System.Reflection;

namespace Dawning.Extensions;

/// <summary>
/// 对象扩展方法
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// 判断对象是否为 null
    /// </summary>
    public static bool IsNull<T>(this T? obj)
        where T : class => obj == null;

    /// <summary>
    /// 判断对象是否不为 null
    /// </summary>
    public static bool IsNotNull<T>(this T? obj)
        where T : class => obj != null;

    /// <summary>
    /// 如果对象为 null，返回默认值
    /// </summary>
    public static T IfNull<T>(this T? obj, T defaultValue)
        where T : class => obj ?? defaultValue;

    /// <summary>
    /// 如果对象为 null，执行工厂方法获取默认值
    /// </summary>
    public static T IfNull<T>(this T? obj, Func<T> defaultFactory)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(defaultFactory);
        return obj ?? defaultFactory();
    }

    /// <summary>
    /// 如果条件为真，执行操作并返回对象
    /// </summary>
    public static T When<T>(this T obj, bool condition, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (condition)
            action(obj);
        return obj;
    }

    /// <summary>
    /// 如果条件为真，执行转换并返回结果
    /// </summary>
    public static T When<T>(this T obj, bool condition, Func<T, T> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        return condition ? transform(obj) : obj;
    }

    /// <summary>
    /// 将对象转换为字典
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
    /// 检查对象是否在指定的值列表中
    /// </summary>
    public static bool IsIn<T>(this T obj, params T[] values) => values.Contains(obj);

    /// <summary>
    /// 检查对象是否不在指定的值列表中
    /// </summary>
    public static bool IsNotIn<T>(this T obj, params T[] values) => !values.Contains(obj);

    /// <summary>
    /// 执行操作并返回对象（链式调用）
    /// </summary>
    public static T Tap<T>(this T obj, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action(obj);
        return obj;
    }

    /// <summary>
    /// 转换对象
    /// </summary>
    public static TResult Pipe<T, TResult>(this T obj, Func<T, TResult> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        return transform(obj);
    }

    /// <summary>
    /// 尝试将对象转换为指定类型
    /// </summary>
    public static T? As<T>(this object? obj)
        where T : class => obj as T;

    /// <summary>
    /// 获取枚举的 Description 特性值
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// 安全地获取属性值
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
    /// 安全地设置属性值
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
    /// 将值限制在指定范围内
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
    /// 检查值是否在指定范围内
    /// </summary>
    public static bool IsBetween<T>(this T value, T min, T max, bool inclusive = true)
        where T : IComparable<T>
    {
        if (inclusive)
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        return value.CompareTo(min) > 0 && value.CompareTo(max) < 0;
    }
}
