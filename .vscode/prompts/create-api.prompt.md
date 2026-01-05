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

### 3. 创建 Mapper 文件
在 `Mapping/{Module}/` 目录创建静态 Mapper：

```csharp
namespace YourProject.Application.Mapping;

public class {Resource}Profile : Profile
{
    public {Resource}Profile()
    {
        CreateMap<{Resource}, {Resource}Dto>();
        CreateMap<Create{Resource}Dto, {Resource}>();
    }
}

public static class {Resource}Mappers
{
    private static IMapper Mapper { get; }
    
    static {Resource}Mappers()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<{Resource}Profile>());
        Mapper = config.CreateMapper();
    }

    public static {Resource}Dto ToDto(this {Resource} entity) => Mapper.Map<{Resource}Dto>(entity);
    public static {Resource}Dto? ToDtoOrNull(this {Resource}? entity) => entity?.ToDto();
    public static IEnumerable<{Resource}Dto> ToDtos(this IEnumerable<{Resource}> entities) => 
        entities.Select(e => e.ToDto());
    public static {Resource} ToEntity(this Create{Resource}Dto dto) => Mapper.Map<{Resource}>(dto);
    public static void ApplyUpdate(this {Resource} entity, Update{Resource}Dto dto) => Mapper.Map(dto, entity);
}
```

### 4. 创建 Service 接口和实现
```csharp
public interface I{Resource}Service
{
    Task<{Resource}Dto?> GetByIdAsync(Guid id);
    Task<PagedData<{Resource}Dto>> GetPagedAsync({Resource}QueryModel query, int page, int pageSize);
    Task<{Resource}Dto> CreateAsync(Create{Resource}Dto dto, string? username = null);
    Task<{Resource}Dto?> UpdateAsync(Update{Resource}Dto dto, string? username = null);
    Task<bool> DeleteAsync(Guid id);
}

// ✅ 使用 UnitOfWork + 静态 Mapper
public class {Resource}Service(IUnitOfWork unitOfWork) : I{Resource}Service
{
    public async Task<{Resource}Dto?> GetByIdAsync(Guid id)
    {
        var entity = await unitOfWork.{Resource}.GetByIdAsync(id);
        return entity?.ToDto();  // 使用静态 Mapper
    }

    public async Task<PagedData<{Resource}Dto>> GetPagedAsync({Resource}QueryModel query, int page, int pageSize)
    {
        var pagedData = await unitOfWork.{Resource}.GetPagedListAsync(query, page, pageSize);
        return new PagedData<{Resource}Dto>
        {
            Items = pagedData.Items.ToDtos(),
            TotalCount = pagedData.TotalCount,
            PageIndex = pagedData.PageIndex,
            PageSize = pagedData.PageSize
        };
    }

    public async Task<{Resource}Dto> CreateAsync(Create{Resource}Dto dto, string? username = null)
    {
        var entity = dto.ToEntity();
        entity.CreatedBy = username;
        await unitOfWork.{Resource}.InsertAsync(entity);
        return entity.ToDto();
    }

    public async Task<{Resource}Dto?> UpdateAsync(Update{Resource}Dto dto, string? username = null)
    {
        var entity = await unitOfWork.{Resource}.GetByIdAsync(dto.Id);
        if (entity == null) return null;
        
        entity.ApplyUpdate(dto);
        entity.UpdatedBy = username;
        await unitOfWork.{Resource}.UpdateAsync(entity);
        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await unitOfWork.{Resource}.DeleteAsync(id);
        return result > 0;
    }
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
