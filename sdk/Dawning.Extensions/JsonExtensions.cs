using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dawning.Extensions;

/// <summary>
/// JSON 序列化扩展方法
/// </summary>
public static class JsonExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// 将对象序列化为 JSON 字符串
    /// </summary>
    public static string ToJson<T>(this T obj, bool indented = false)
    {
        if (obj == null)
            return "null";

        var options = indented ? IndentedOptions : DefaultOptions;
        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// 将对象序列化为 JSON 字符串（使用自定义选项）
    /// </summary>
    public static string ToJson<T>(this T obj, JsonSerializerOptions options)
    {
        if (obj == null)
            return "null";

        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为对象
    /// </summary>
    public static T? FromJson<T>(this string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为对象（使用自定义选项）
    /// </summary>
    public static T? FromJson<T>(this string json, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// 尝试将 JSON 字符串反序列化为对象
    /// </summary>
    public static bool TryFromJson<T>(this string json, out T? result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 深度克隆对象（通过 JSON 序列化/反序列化）
    /// </summary>
    public static T? DeepClone<T>(this T obj)
    {
        if (obj == null)
            return default;

        var json = JsonSerializer.Serialize(obj, DefaultOptions);
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// 检查字符串是否是有效的 JSON
    /// </summary>
    public static bool IsValidJson(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 将 JSON 字符串格式化（美化）
    /// </summary>
    public static string? PrettyPrint(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, IndentedOptions);
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// 将 JSON 字符串压缩（移除空白）
    /// </summary>
    public static string? Minify(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, DefaultOptions);
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// 合并两个 JSON 对象
    /// </summary>
    public static string MergeJson(this string baseJson, string overrideJson)
    {
        if (string.IsNullOrWhiteSpace(baseJson))
            return overrideJson ?? "{}";
        if (string.IsNullOrWhiteSpace(overrideJson))
            return baseJson;

        using var baseDoc = JsonDocument.Parse(baseJson);
        using var overrideDoc = JsonDocument.Parse(overrideJson);

        var result = new Dictionary<string, JsonElement>();

        // Add base properties
        foreach (var prop in baseDoc.RootElement.EnumerateObject())
        {
            result[prop.Name] = prop.Value.Clone();
        }

        // Override with new properties
        foreach (var prop in overrideDoc.RootElement.EnumerateObject())
        {
            result[prop.Name] = prop.Value.Clone();
        }

        return result.ToJson();
    }

    /// <summary>
    /// 从 JSON 字符串中获取指定路径的值
    /// </summary>
    public static T? GetJsonValue<T>(this string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            var segments = path.Split('.');
            foreach (var segment in segments)
            {
                if (element.TryGetProperty(segment, out var property))
                {
                    element = property;
                }
                else
                {
                    return default;
                }
            }

            return JsonSerializer.Deserialize<T>(element.GetRawText(), DefaultOptions);
        }
        catch
        {
            return default;
        }
    }
}
