---
description: "Create RESTful API endpoint for Dawning: DTO, Mapper, Service, Controller, DI registration. Trigger: 创建API, create api, 新建接口, 新增端点, endpoint, controller, 后端接口"
---

# Create API Skill

## 目标

创建完整的 RESTful API 端点，包括 DTO、Mapper、Service、Controller。

## 触发条件

- **关键词**：创建API, create api, 新建接口, 新增端点, endpoint, controller, 后端接口, CRUD
- **文件模式**：`*Controller.cs`, `*Service.cs`, `*Dto.cs`
- **用户意图**：创建新的后端 API 端点、新增 CRUD 接口

## 编排

- **前置**：`create-database`（先有表结构）
- **后续**：`create-tests`（创建后写测试）

---

## 创建流程

### 1. 创建 DTO 文件

```csharp
// Dtos/{Resource}Dto.cs
public class {Resource}Dto
{
    public Guid Id { get; set; }
    public long Timestamp { get; set; }
    // 业务字段
}

// Dtos/Create{Resource}Dto.cs
public class Create{Resource}Dto
{
    // 创建所需字段（不含 Id、Timestamp）
}

// Dtos/Update{Resource}Dto.cs
public class Update{Resource}Dto
{
    public Guid Id { get; set; }
    // 可更新字段
}

// Dtos/{Resource}QueryDto.cs
public class {Resource}QueryDto
{
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

### 2. 创建 Mapper

参考 `code-patterns` skill 中的静态 Mapper 模板。

### 3. 创建 Service

```csharp
public interface I{Resource}Service
{
    Task<{Resource}Dto?> GetByIdAsync(Guid id);
    Task<PagedData<{Resource}Dto>> GetPagedAsync({Resource}QueryDto query);
    Task<{Resource}Dto> CreateAsync(Create{Resource}Dto dto, string? username = null);
    Task<{Resource}Dto?> UpdateAsync(Update{Resource}Dto dto, string? username = null);
    Task<bool> DeleteAsync(Guid id);
}

public class {Resource}Service(IUnitOfWork unitOfWork) : I{Resource}Service
{
    public async Task<{Resource}Dto?> GetByIdAsync(Guid id)
    {
        var entity = await unitOfWork.{Resource}.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<{Resource}Dto> CreateAsync(Create{Resource}Dto dto, string? username = null)
    {
        var entity = dto.ToEntity();
        entity.CreatedBy = username;
        await unitOfWork.{Resource}.InsertAsync(entity);
        return entity.ToDto();
    }

    // UpdateAsync, DeleteAsync, GetPagedAsync...
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
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResult.Success(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Update{Resource}Dto dto)
    {
        await service.UpdateAsync(dto);
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

### 5. 注册 DI

```csharp
services.AddScoped<I{Resource}Service, {Resource}Service>();
```

## 规范要求

- 使用 `ApiResult<T>` 统一返回格式
- 使用 primary constructor (C# 12)
- 使用静态 Mapper（不注入 IMapper）
- 通过 UnitOfWork 访问 Repository

## 验收场景

- **输入**："创建一个 Product 的 CRUD API"
- **预期**：agent 生成 DTO、Mapper、Service、Controller 全套代码
- **上次验证**：2026-02-27 ✅
