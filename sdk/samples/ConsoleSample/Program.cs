using Dawning.Core.Exceptions;
using Dawning.Core.Results;
using Dawning.Extensions;

Console.WriteLine("========================================");
Console.WriteLine("  Dawning SDK Console Sample");
Console.WriteLine("========================================");
Console.WriteLine();

// ========================================
// 1. 字符串扩展演示
// ========================================
Console.WriteLine("【字符串扩展】");
Console.WriteLine();

var text = "HelloWorld";
Console.WriteLine($"原始文本: {text}");
Console.WriteLine($"  ToPascalCase: {"hello_world".ToPascalCase()}");
Console.WriteLine($"  ToCamelCase:  {text.ToCamelCase()}");
Console.WriteLine($"  ToSnakeCase:  {text.ToSnakeCase()}");
Console.WriteLine($"  ToKebabCase:  {text.ToKebabCase()}");
Console.WriteLine();

var phone = "13812345678";
var email = "test@example.com";
Console.WriteLine($"手机号: {phone}");
Console.WriteLine($"  IsValidPhoneNumber: {phone.IsValidPhoneNumber()}");
Console.WriteLine($"  Mask: {phone.Mask()}");
Console.WriteLine();
Console.WriteLine($"邮箱: {email}");
Console.WriteLine($"  IsValidEmail: {email.IsValidEmail()}");
Console.WriteLine();

var longText = "这是一段很长很长的文本，需要被截断显示";
Console.WriteLine($"长文本: {longText}");
Console.WriteLine($"  Truncate(10): {longText.Truncate(10)}");
Console.WriteLine();

// ========================================
// 2. 日期时间扩展演示
// ========================================
Console.WriteLine("【日期时间扩展】");
Console.WriteLine();

var now = DateTime.Now;
var birthDate = new DateTime(1990, 5, 15);

Console.WriteLine($"当前时间: {now.ToDateTimeString()}");
Console.WriteLine($"  StartOfDay:   {now.StartOfDay().ToDateTimeString()}");
Console.WriteLine($"  EndOfDay:     {now.EndOfDay().ToDateTimeString()}");
Console.WriteLine($"  StartOfMonth: {now.StartOfMonth().ToDateString()}");
Console.WriteLine($"  EndOfMonth:   {now.EndOfMonth().ToDateString()}");
Console.WriteLine($"  IsWeekend:    {now.IsWeekend()}");
Console.WriteLine($"  IsWeekday:    {now.IsWeekday()}");
Console.WriteLine();

Console.WriteLine($"出生日期: {birthDate.ToDateString()}");
Console.WriteLine($"  年龄: {birthDate.CalculateAge()} 岁");
Console.WriteLine();

Console.WriteLine($"相对时间示例:");
Console.WriteLine($"  1分钟前:  {now.AddMinutes(-1).ToRelativeTime()}");
Console.WriteLine($"  3小时前:  {now.AddHours(-3).ToRelativeTime()}");
Console.WriteLine($"  5天前:    {now.AddDays(-5).ToRelativeTime()}");
Console.WriteLine($"  2个月前:  {now.AddMonths(-2).ToRelativeTime()}");
Console.WriteLine();

Console.WriteLine($"Unix 时间戳:");
Console.WriteLine($"  秒:   {now.ToUnixTimeSeconds()}");
Console.WriteLine($"  毫秒: {now.ToUnixTimeMilliseconds()}");
Console.WriteLine();

// ========================================
// 3. 集合扩展演示
// ========================================
Console.WriteLine("【集合扩展】");
Console.WriteLine();

var numbers = Enumerable.Range(1, 20).ToList();
Console.WriteLine($"原始数组: {numbers.JoinToString(", ")}");
Console.WriteLine();

Console.WriteLine("  Batch(5) 分批处理:");
var batchIndex = 0;
foreach (var batch in numbers.Batch(5))
{
    Console.WriteLine($"    批次 {++batchIndex}: {batch.JoinToString(", ")}");
}
Console.WriteLine();

Console.WriteLine($"  Shuffle().Take(5): {numbers.Shuffle().Take(5).JoinToString(", ")}");
Console.WriteLine($"  RandomElement: {numbers.RandomElement()}");
Console.WriteLine();

var dict1 = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
var dict2 = new Dictionary<string, int> { ["b"] = 3, ["c"] = 4 };
Console.WriteLine($"字典合并:");
Console.WriteLine($"  dict1: {dict1.Select(kv => $"{kv.Key}={kv.Value}").JoinToString(", ")}");
Console.WriteLine($"  dict2: {dict2.Select(kv => $"{kv.Key}={kv.Value}").JoinToString(", ")}");
Console.WriteLine(
    $"  Merge: {dict1.Merge(dict2).Select(kv => $"{kv.Key}={kv.Value}").JoinToString(", ")}"
);
Console.WriteLine();

// ========================================
// 4. JSON 扩展演示
// ========================================
Console.WriteLine("【JSON 扩展】");
Console.WriteLine();

var user = new
{
    Name = "张三",
    Age = 25,
    Email = "test@example.com",
};
var json = user.ToJson();
var prettyJson = user.ToJson(indented: true);

Console.WriteLine($"对象: {user}");
Console.WriteLine($"  ToJson():         {json}");
Console.WriteLine($"  ToJson(indented):");
Console.WriteLine(prettyJson);
Console.WriteLine();

Console.WriteLine($"  IsValidJson: {json.IsValidJson()}");
Console.WriteLine($"  深克隆: {user.DeepClone()?.ToJson()}");
Console.WriteLine();

// ========================================
// 5. 对象扩展演示
// ========================================
Console.WriteLine("【对象扩展】");
Console.WriteLine();

var value = 50;
Console.WriteLine($"数值: {value}");
Console.WriteLine($"  Clamp(0, 30):      {value.Clamp(0, 30)}");
Console.WriteLine($"  IsBetween(40, 60): {value.IsBetween(40, 60)}");
Console.WriteLine($"  IsIn(10, 50, 100): {value.IsIn(10, 50, 100)}");
Console.WriteLine();

string? nullString = null;
Console.WriteLine($"空字符串处理:");
Console.WriteLine($"  IfNullOrWhiteSpace: {nullString.IfNullOrWhiteSpace("默认值")}");
Console.WriteLine();

// ========================================
// 6. ApiResult 演示
// ========================================
Console.WriteLine("【ApiResult 统一响应】");
Console.WriteLine();

var successResult = ApiResults.Ok(new { Id = 1, Name = "测试" }, "操作成功");
var failResult = ApiResults.Fail<object>("操作失败", "ERR_001");

Console.WriteLine($"成功响应: {successResult.ToJson(indented: true)}");
Console.WriteLine();
Console.WriteLine($"失败响应: {failResult.ToJson(indented: true)}");
Console.WriteLine();

// ========================================
// 7. 业务异常演示
// ========================================
Console.WriteLine("【业务异常】");
Console.WriteLine();

try
{
    // 模拟验证失败
    throw new ValidationException("用户名不能为空");
}
catch (BusinessException ex)
{
    Console.WriteLine($"捕获业务异常:");
    Console.WriteLine($"  类型: {ex.GetType().Name}");
    Console.WriteLine($"  消息: {ex.Message}");
    Console.WriteLine($"  状态码: {(int)ex.StatusCode}");
}
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("  演示完成！");
Console.WriteLine("========================================");
