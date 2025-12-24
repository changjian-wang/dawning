# Dawning.Extensions

通用扩展方法库，提供丰富的实用工具方法。

## 安装

```bash
dotnet add package Dawning.Extensions
```

## 功能模块

### 🔤 StringExtensions - 字符串扩展

```csharp
using Dawning.Extensions;

// 空值检查
"hello".IsNullOrWhiteSpace();    // false
"".IfNullOrWhiteSpace("default"); // "default"

// 字符串截断
"这是一个很长的字符串".Truncate(5);  // "这是..."

// 命名转换
"hello_world".ToPascalCase();     // "HelloWorld"
"HelloWorld".ToCamelCase();       // "helloWorld"
"HelloWorld".ToSnakeCase();       // "hello_world"
"HelloWorld".ToKebabCase();       // "hello-world"

// 验证
"test@example.com".IsValidEmail();   // true
"13812345678".IsValidPhoneNumber();  // true

// 隐私掩码
"13812345678".Mask();  // "138****5678"
```

### 📚 CollectionExtensions - 集合扩展

```csharp
using Dawning.Extensions;

// 空值检查
list.IsNullOrEmpty();
list.OrEmpty();  // 如果为 null 返回空集合

// 遍历
list.ForEach(item => Console.WriteLine(item));
list.ForEach((item, index) => Console.WriteLine($"{index}: {item}"));

// 分批处理
items.Batch(100).ForEach(batch => ProcessBatch(batch));

// 去重
users.DistinctBy(u => u.Email);

// 随机
items.Shuffle();
items.RandomElement();

// 字典操作
dict.GetValueOrDefault("key", "default");
dict1.Merge(dict2);

// 连接
list.JoinToString(", ");  // "a, b, c"
```

### 📅 DateTimeExtensions - 日期时间扩展

```csharp
using Dawning.Extensions;

// 时间边界
DateTime.Now.StartOfDay();    // 今天 00:00:00
DateTime.Now.EndOfDay();      // 今天 23:59:59
DateTime.Now.StartOfMonth();  // 本月第一天
DateTime.Now.EndOfMonth();    // 本月最后一天

// 判断
DateTime.Now.IsWeekend();   // 是否周末
DateTime.Now.IsWeekday();   // 是否工作日

// 计算
birthDate.CalculateAge();   // 计算年龄
date.AddWorkdays(5);        // 添加工作日

// Unix 时间戳
DateTime.Now.ToUnixTimeSeconds();
DateTime.Now.ToUnixTimeMilliseconds();
timestamp.FromUnixTimeSeconds();

// 相对时间
dateTime.ToRelativeTime();  // "3分钟前"、"2天前"

// 格式化
DateTime.Now.ToDateString();      // "2024-01-15"
DateTime.Now.ToDateTimeString();  // "2024-01-15 10:30:00"
DateTime.Now.ToIso8601();         // "2024-01-15T10:30:00.000Z"
```

### 📄 JsonExtensions - JSON 扩展

```csharp
using Dawning.Extensions;

// 序列化
var json = user.ToJson();           // 压缩
var json = user.ToJson(indented: true);  // 格式化

// 反序列化
var user = json.FromJson<User>();
if (json.TryFromJson<User>(out var result)) { }

// 深克隆
var clone = user.DeepClone();

// JSON 验证
json.IsValidJson();

// 格式化
json.PrettyPrint();  // 美化
json.Minify();       // 压缩

// 合并 JSON
baseJson.MergeJson(overrideJson);

// 获取嵌套值
json.GetJsonValue<string>("user.profile.name");
```

### 🎯 ObjectExtensions - 对象扩展

```csharp
using Dawning.Extensions;

// 空值检查
obj.IsNull();
obj.IsNotNull();
obj.IfNull(defaultValue);
obj.IfNull(() => CreateDefault());

// 条件执行
user.When(needUpdate, u => u.UpdatedAt = DateTime.Now);

// 转换
obj.ToDictionary();
obj.As<User>();

// 范围检查
value.IsIn(1, 2, 3);
value.IsBetween(0, 100);
value.Clamp(0, 100);

// 链式调用
user
    .Tap(u => Logger.Log($"Processing {u.Name}"))
    .Pipe(u => new UserDto(u));

// 枚举描述
MyEnum.Value.GetDescription();

// 反射操作
obj.GetPropertyValue("Name");
obj.SetPropertyValue("Name", "New Value");
```

### ⏰ TimestampUtil - 时间戳工具

```csharp
using Dawning.Extensions;

// 获取当前时间戳（毫秒）
var timestamp = TimestampUtil.GetCurrentTimestamp();
```

## 许可证

MIT License
