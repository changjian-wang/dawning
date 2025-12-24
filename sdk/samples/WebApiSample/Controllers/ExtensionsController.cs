using Dawning.Core.Results;
using Dawning.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSample.Controllers;

/// <summary>
/// 扩展方法演示控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExtensionsController : ControllerBase
{
    /// <summary>
    /// 字符串扩展演示
    /// </summary>
    [HttpGet("string")]
    public ActionResult<ApiResult<object>> StringExtensions([FromQuery] string input = "HelloWorld")
    {
        var result = new
        {
            Original = input,
            ToPascalCase = input.ToPascalCase(),
            ToCamelCase = input.ToCamelCase(),
            ToSnakeCase = input.ToSnakeCase(),
            ToKebabCase = input.ToKebabCase(),
            Truncated = input.Truncate(5),
            IsValidEmail = input.IsValidEmail(),
            RemoveWhitespace = input.RemoveWhitespace()
        };
        
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>
    /// 日期时间扩展演示
    /// </summary>
    [HttpGet("datetime")]
    public ActionResult<ApiResult<object>> DateTimeExtensions()
    {
        var now = DateTime.Now;
        var birthDate = new DateTime(1990, 5, 15);
        
        var result = new
        {
            Now = now.ToDateTimeString(),
            StartOfDay = now.StartOfDay().ToDateTimeString(),
            EndOfDay = now.EndOfDay().ToDateTimeString(),
            StartOfMonth = now.StartOfMonth().ToDateString(),
            EndOfMonth = now.EndOfMonth().ToDateString(),
            StartOfWeek = now.StartOfWeek().ToDateString(),
            IsWeekend = now.IsWeekend(),
            IsWeekday = now.IsWeekday(),
            Age = birthDate.CalculateAge(),
            UnixTimestamp = now.ToUnixTimeSeconds(),
            RelativeTime = now.AddHours(-3).ToRelativeTime(),
            Iso8601 = now.ToIso8601()
        };
        
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>
    /// JSON 扩展演示
    /// </summary>
    [HttpPost("json")]
    public ActionResult<ApiResult<object>> JsonExtensions([FromBody] object input)
    {
        var json = input.ToJson();
        var prettyJson = json.PrettyPrint();
        var isValid = json.IsValidJson();
        
        var result = new
        {
            Minified = json,
            Pretty = prettyJson,
            IsValid = isValid,
            DeepCloned = input.DeepClone()
        };
        
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>
    /// 集合扩展演示
    /// </summary>
    [HttpGet("collection")]
    public ActionResult<ApiResult<object>> CollectionExtensions()
    {
        var numbers = Enumerable.Range(1, 20).ToList();
        var words = new List<string> { "apple", "banana", "cherry" };
        var dict1 = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var dict2 = new Dictionary<string, int> { ["b"] = 3, ["c"] = 4 };
        
        var result = new
        {
            Original = numbers,
            Batched = numbers.Batch(5).Select(b => b.ToList()).ToList(),
            Shuffled = numbers.Shuffle().Take(5).ToList(),
            RandomElement = numbers.RandomElement(),
            JoinedWords = words.JoinToString(" | "),
            MergedDict = dict1.Merge(dict2),
            IsNullOrEmpty = new List<int>().IsNullOrEmpty()
        };
        
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>
    /// 对象扩展演示
    /// </summary>
    [HttpGet("object")]
    public ActionResult<ApiResult<object>> ObjectExtensions()
    {
        var value = 50;
        string? nullString = null;
        var testObj = new { Name = "Test", Age = 25 };
        
        var result = new
        {
            Clamped = value.Clamp(0, 30),
            IsBetween = value.IsBetween(40, 60),
            IsIn = value.IsIn(10, 20, 50, 100),
            IfNullOrWhiteSpace = nullString.IfNullOrWhiteSpace("默认值"),
            ToDictionary = testObj.ToDictionary(),
            PropertyValue = testObj.GetPropertyValue("Name")
        };
        
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>
    /// 手机号掩码演示
    /// </summary>
    [HttpGet("mask")]
    public ActionResult<ApiResult<object>> MaskDemo([FromQuery] string phone = "13812345678")
    {
        var result = new
        {
            Original = phone,
            Masked = phone.Mask(),
            IsValidPhone = phone.IsValidPhoneNumber()
        };
        
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>
    /// 时间戳工具演示
    /// </summary>
    [HttpGet("timestamp")]
    public ActionResult<ApiResult<object>> TimestampDemo()
    {
        var currentTs = TimestampUtil.GetCurrentTimestamp();
        
        var result = new
        {
            CurrentTimestamp = currentTs,
            FromTimestamp = currentTs.FromUnixTimeMilliseconds().ToDateTimeString(),
            ToUnixSeconds = DateTime.UtcNow.ToUnixTimeSeconds(),
            ToUnixMilliseconds = DateTime.UtcNow.ToUnixTimeMilliseconds()
        };
        
        return Ok(ApiResults.Ok(result));
    }
}
