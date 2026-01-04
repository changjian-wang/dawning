---
description: 创建新的后端 API 端点
---

# 创建 ASP.NET Core API 端点

根据用户需求创建完整的 RESTful API 端点。

## 创建流程

### 1. 分析需求
- 理解要创建的资源类型
- 确定需要哪些 CRUD 操作
- 识别业务规则和验证需求

### 2. 创建 DTO 文件
在 `Dtos/` 目录创建：
- `{Resource}Dto.cs` - 返回数据的 DTO
- `Create{Resource}Dto.cs` - 创建请求 DTO
- `Update{Resource}Dto.cs` - 更新请求 DTO
- `{Resource}QueryDto.cs` - 查询参数 DTO

```csharp
/// <summary>
/// {Resource} 数据传输对象
/// </summary>
public class {Resource}Dto
{
    public Guid Id { get; set; }
    public long Timestamp { get; set; }
    // 其他字段
}
```

### 3. 创建 Service 接口和实现
```csharp
public interface I{Resource}Service
{
    Task<{Resource}Dto?> GetByIdAsync(Guid id);
    Task<PagedResult<{Resource}Dto>> GetPagedAsync({Resource}QueryDto query);
    Task<Guid> CreateAsync(Create{Resource}Dto dto);
    Task UpdateAsync(Guid id, Update{Resource}Dto dto);
    Task DeleteAsync(Guid id);
}
```

### 4. 创建 Controller
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class {Resource}Controller(I{Resource}Service service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<PagedResult<{Resource}Dto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] {Resource}QueryDto query)
    {
        var result = await service.GetPagedAsync(query);
        return Ok(ApiResult.Success(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await service.GetByIdAsync(id);
        return item == null 
            ? NotFound(ApiResult.NotFound("{Resource} not found"))
            : Ok(ApiResult.Success(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] Create{Resource}Dto dto)
    {
        var id = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResult.Success(id));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Update{Resource}Dto dto)
    {
        await service.UpdateAsync(id, dto);
        return Ok(ApiResult.Success());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
```

### 5. 注册依赖注入
在 DI 配置中添加服务注册。

## 规范要求

- 使用 `ApiResult<T>` 统一返回格式
- 添加 XML 文档注释
- 使用 `[Authorize]` 保护端点
- 添加 `[ProducesResponseType]` 声明响应类型
- 使用 primary constructor (C# 12)
- 遵循 RESTful 命名规范
