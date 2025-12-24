# Dawning.Core

统一 API 响应和异常处理库。

## 安装

```bash
dotnet add package Dawning.Core
```

## 功能

- **统一 API 响应** - `ApiResult<T>` 和 `PagedResult<T>`
- **业务异常** - 预定义的异常类型
- **异常处理中间件** - 自动捕获并格式化异常响应

## 使用

### 注册中间件

```csharp
app.UseDawningExceptionHandling();
```

### 返回统一响应

```csharp
[HttpGet]
public ApiResult<UserDto> GetUser(int id)
{
    var user = _userService.GetById(id);
    return ApiResult<UserDto>.Success(user);
}

// 分页响应
[HttpGet]
public PagedResult<UserDto> GetUsers(int page, int pageSize)
{
    var (items, total) = _userService.GetPaged(page, pageSize);
    return PagedResult<UserDto>.Success(items, total, page, pageSize);
}
```

### 抛出业务异常

```csharp
throw new NotFoundException("用户不存在");
throw new BusinessException("操作失败");
throw new ValidationException("参数无效");
throw new UnauthorizedException("未授权");
throw new ForbiddenException("禁止访问");
```

## 响应格式

```json
{
  "success": true,
  "code": 200,
  "message": "操作成功",
  "data": { ... }
}
```
