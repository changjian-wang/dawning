using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dawning.Extensions;

/// <summary>
/// JSON serialization extension methods
/// </summary>
public static class JsonExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>
    /// Serializes the object to a JSON string
    /// </summary>
    public static string ToJson<T>(this T obj, bool indented = false)
    {
        if (obj == null)
            return "null";

        var options = indented ? IndentedOptions : DefaultOptions;
        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// Serializes the object to a JSON string (with custom options)
    /// </summary>
    public static string ToJson<T>(this T obj, JsonSerializerOptions options)
    {
        if (obj == null)
            return "null";

        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// Deserializes the JSON string to an object
    /// </summary>
    public static T? FromJson<T>(this string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// Deserializes the JSON string to an object (with custom options)
    /// </summary>
    public static T? FromJson<T>(this string json, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// Tries to deserialize the JSON string to an object
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
    /// Deep clones the object (via JSON serialization/deserialization)
    /// </summary>
    public static T? DeepClone<T>(this T obj)
    {
        if (obj == null)
            return default;

        var json = JsonSerializer.Serialize(obj, DefaultOptions);
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// Checks if the string is valid JSON
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
    /// Formats (prettifies) the JSON string
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
    /// Minifies the JSON string (removes whitespace)
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
    /// Merges two JSON objects
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
    /// Gets the value at the specified path from a JSON string
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
