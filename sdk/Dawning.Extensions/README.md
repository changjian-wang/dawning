# Dawning.Extensions

通用扩展方法库。

## 安装

```bash
dotnet add package Dawning.Extensions
```

## 功能

- **时间戳工具** - Unix 时间戳转换

## 使用

### 时间戳转换

```csharp
using Dawning.Extensions;

// DateTime → 时间戳（秒）
long timestamp = TimestampUtil.ToTimestamp(DateTime.UtcNow);

// 时间戳 → DateTime
DateTime dateTime = TimestampUtil.FromTimestamp(timestamp);

// DateTime → 时间戳（毫秒）
long ms = TimestampUtil.ToMilliseconds(DateTime.UtcNow);

// 毫秒时间戳 → DateTime
DateTime dt = TimestampUtil.FromMilliseconds(ms);
```
