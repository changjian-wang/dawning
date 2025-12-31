using Dawning.Core.Exceptions;
using Dawning.Core.Results;
using Dawning.Extensions;

Console.WriteLine("========================================");
Console.WriteLine("  Dawning SDK Console Sample");
Console.WriteLine("========================================");
Console.WriteLine();

// ========================================
// 1. String Extensions Demo
// ========================================
Console.WriteLine("[String Extensions]");
Console.WriteLine();

var text = "HelloWorld";
Console.WriteLine($"Original text: {text}");
Console.WriteLine($"  ToPascalCase: {"hello_world".ToPascalCase()}");
Console.WriteLine($"  ToCamelCase:  {text.ToCamelCase()}");
Console.WriteLine($"  ToSnakeCase:  {text.ToSnakeCase()}");
Console.WriteLine($"  ToKebabCase:  {text.ToKebabCase()}");
Console.WriteLine();

var phone = "13812345678";
var email = "test@example.com";
Console.WriteLine($"Phone number: {phone}");
Console.WriteLine($"  IsValidPhoneNumber: {phone.IsValidPhoneNumber()}");
Console.WriteLine($"  Mask: {phone.Mask()}");
Console.WriteLine();
Console.WriteLine($"Email: {email}");
Console.WriteLine($"  IsValidEmail: {email.IsValidEmail()}");
Console.WriteLine();

var longText = "This is a very long text that needs to be truncated for display";
Console.WriteLine($"Long text: {longText}");
Console.WriteLine($"  Truncate(10): {longText.Truncate(10)}");
Console.WriteLine();

// ========================================
// 2. DateTime Extensions Demo
// ========================================
Console.WriteLine("[DateTime Extensions]");
Console.WriteLine();

var now = DateTime.Now;
var birthDate = new DateTime(1990, 5, 15);

Console.WriteLine($"Current time: {now.ToDateTimeString()}");
Console.WriteLine($"  StartOfDay:   {now.StartOfDay().ToDateTimeString()}");
Console.WriteLine($"  EndOfDay:     {now.EndOfDay().ToDateTimeString()}");
Console.WriteLine($"  StartOfMonth: {now.StartOfMonth().ToDateString()}");
Console.WriteLine($"  EndOfMonth:   {now.EndOfMonth().ToDateString()}");
Console.WriteLine($"  IsWeekend:    {now.IsWeekend()}");
Console.WriteLine($"  IsWeekday:    {now.IsWeekday()}");
Console.WriteLine();

Console.WriteLine($"Birth date: {birthDate.ToDateString()}");
Console.WriteLine($"  Age: {birthDate.CalculateAge()} years old");
Console.WriteLine();

Console.WriteLine($"Relative time examples:");
Console.WriteLine($"  1 minute ago:  {now.AddMinutes(-1).ToRelativeTime()}");
Console.WriteLine($"  3 hours ago:   {now.AddHours(-3).ToRelativeTime()}");
Console.WriteLine($"  5 days ago:    {now.AddDays(-5).ToRelativeTime()}");
Console.WriteLine($"  2 months ago:  {now.AddMonths(-2).ToRelativeTime()}");
Console.WriteLine();

Console.WriteLine($"Unix timestamp:");
Console.WriteLine($"  Seconds:      {now.ToUnixTimeSeconds()}");
Console.WriteLine($"  Milliseconds: {now.ToUnixTimeMilliseconds()}");
Console.WriteLine();

// ========================================
// 3. Collection Extensions Demo
// ========================================
Console.WriteLine("[Collection Extensions]");
Console.WriteLine();

var numbers = Enumerable.Range(1, 20).ToList();
Console.WriteLine($"Original array: {numbers.JoinToString(", ")}");
Console.WriteLine();

Console.WriteLine("  Batch(5) batch processing:");
var batchIndex = 0;
foreach (var batch in numbers.Batch(5))
{
    Console.WriteLine($"    Batch {++batchIndex}: {batch.JoinToString(", ")}");
}
Console.WriteLine();

Console.WriteLine($"  Shuffle().Take(5): {numbers.Shuffle().Take(5).JoinToString(", ")}");
Console.WriteLine($"  RandomElement: {numbers.RandomElement()}");
Console.WriteLine();

var dict1 = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
var dict2 = new Dictionary<string, int> { ["b"] = 3, ["c"] = 4 };
Console.WriteLine($"Dictionary merge:");
Console.WriteLine($"  dict1: {dict1.Select(kv => $"{kv.Key}={kv.Value}").JoinToString(", ")}");
Console.WriteLine($"  dict2: {dict2.Select(kv => $"{kv.Key}={kv.Value}").JoinToString(", ")}");
Console.WriteLine(
    $"  Merge: {dict1.Merge(dict2).Select(kv => $"{kv.Key}={kv.Value}").JoinToString(", ")}"
);
Console.WriteLine();

// ========================================
// 4. JSON Extensions Demo
// ========================================
Console.WriteLine("[JSON Extensions]");
Console.WriteLine();

var user = new
{
    Name = "John Doe",
    Age = 25,
    Email = "test@example.com",
};
var json = user.ToJson();
var prettyJson = user.ToJson(indented: true);

Console.WriteLine($"Object: {user}");
Console.WriteLine($"  ToJson():         {json}");
Console.WriteLine($"  ToJson(indented):");
Console.WriteLine(prettyJson);
Console.WriteLine();

Console.WriteLine($"  IsValidJson: {json.IsValidJson()}");
Console.WriteLine($"  DeepClone: {user.DeepClone()?.ToJson()}");
Console.WriteLine();

// ========================================
// 5. Object Extensions Demo
// ========================================
Console.WriteLine("[Object Extensions]");
Console.WriteLine();

var value = 50;
Console.WriteLine($"Value: {value}");
Console.WriteLine($"  Clamp(0, 30):      {value.Clamp(0, 30)}");
Console.WriteLine($"  IsBetween(40, 60): {value.IsBetween(40, 60)}");
Console.WriteLine($"  IsIn(10, 50, 100): {value.IsIn(10, 50, 100)}");
Console.WriteLine();

string? nullString = null;
Console.WriteLine($"Null string handling:");
Console.WriteLine($"  IfNullOrWhiteSpace: {nullString.IfNullOrWhiteSpace("Default value")}");
Console.WriteLine();

// ========================================
// 6. ApiResult Demo
// ========================================
Console.WriteLine("[ApiResult Unified Response]");
Console.WriteLine();

var successResult = ApiResults.Ok(new { Id = 1, Name = "Test" }, "Operation successful");
var failResult = ApiResults.Fail<object>("Operation failed", "ERR_001");

Console.WriteLine($"Success response: {successResult.ToJson(indented: true)}");
Console.WriteLine();
Console.WriteLine($"Failure response: {failResult.ToJson(indented: true)}");
Console.WriteLine();

// ========================================
// 7. Business Exception Demo
// ========================================
Console.WriteLine("[Business Exception]");
Console.WriteLine();

try
{
    // Simulate validation failure
    throw new ValidationException("Username cannot be empty");
}
catch (BusinessException ex)
{
    Console.WriteLine($"Caught business exception:");
    Console.WriteLine($"  Type: {ex.GetType().Name}");
    Console.WriteLine($"  Message: {ex.Message}");
    Console.WriteLine($"  StatusCode: {(int)ex.StatusCode}");
}
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("  Demo completed!");
Console.WriteLine("========================================");
