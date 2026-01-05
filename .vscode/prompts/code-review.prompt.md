---
description: 代码审查 - 检查代码质量和规范
---

# 代码审查

对代码进行全面审查，检查是否符合项目规范和最佳实践。

## 审查维度

### 1. 代码风格
- [ ] 使用 file-scoped namespaces
- [ ] 使用 primary constructors (C# 12)
- [ ] 私有字段使用 `_camelCase` 命名
- [ ] 公共成员使用 PascalCase
- [ ] 添加 XML 文档注释

### 2. 依赖注入一致性
- [ ] UnitOfWork 字段统一命名为 `_unitOfWork`
- [ ] 通过 `_unitOfWork.{Repository}` 访问 Repository
- [ ] 不要同时注入 Repository 和 UnitOfWork（冗余）
- [ ] 只读操作可单独注入 Repository（如 GatewayConfigService）

### 3. 对象映射规范
- [ ] 使用静态 Mapper 类，不注入 IMapper
- [ ] Mapper 类放在 `Mapping/{Module}/` 目录下
- [ ] 使用扩展方法：`entity.ToDto()`, `dto.ToEntity()`, `entity.ApplyUpdate(dto)`
- [ ] 硬编码字符串使用常量类（如 AuditConstants）

### 4. API 设计
- [ ] 使用 `ApiResult<T>` 统一返回格式
- [ ] 添加 `[Authorize]` 保护端点
- [ ] 添加 `[ProducesResponseType]` 声明响应类型
- [ ] 遵循 RESTful 命名规范
- [ ] 正确使用 HTTP 状态码

### 5. 数据库规范
- [ ] GUID 主键表有 `timestamp` 字段
- [ ] `timestamp` 字段有索引
- [ ] 表名使用 snake_case
- [ ] 布尔字段使用 `is_` 前缀

### 6. 安全性
- [ ] 不硬编码敏感信息
- [ ] 输入参数有验证
- [ ] SQL 使用参数化查询
- [ ] 敏感操作有权限检查

### 7. 性能
- [ ] 使用 async/await
- [ ] 避免 N+1 查询
- [ ] 合理使用缓存
- [ ] 避免大量数据内存加载

### 8. 异常处理
- [ ] 不吞掉异常
- [ ] 使用业务异常类
- [ ] 合理的日志记录
- [ ] 返回有意义的错误信息

### 9. 单元测试规范
- [ ] 测试中通过 `_unitOfWorkMock.Setup(x => x.{Repository})` 设置 Mock
- [ ] 不直接注入 Repository Mock 到 Service 构造函数
- [ ] 移除不再使用的 `IMapper` Mock
- [ ] 测试命名遵循 `方法名_场景_期望结果` 格式

## 审查输出格式

```markdown
## 审查结果

### ✅ 符合规范
- [符合的项目...]

### ⚠️ 建议改进
- **位置**: `文件:行号`
- **问题**: 描述问题
- **建议**: 改进建议
- **严重程度**: 低/中/高

### ❌ 必须修改
- **位置**: `文件:行号`
- **问题**: 描述问题
- **修改建议**: 具体修改代码示例

### 📊 总结
- 代码质量评分: X/10
- 主要问题: ...
- 改进优先级: ...
```

## C# 常见问题检查

### 命名问题
```csharp
// ❌ 错误
private ILogger logger;          // 应该用 _logger
public string username { get; }  // 应该用 Username

// ✅ 正确
private readonly ILogger _logger;
public string Username { get; }
```

### 依赖注入问题
```csharp
// ❌ 错误 - 冗余注入，_uow.User 已可访问 IUserRepository
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;
    
    public UserService(IUserRepository repo, IUnitOfWork uow) { ... }
}

// ❌ 错误 - 命名不统一
private readonly IUnitOfWork _uow;  // 应该用 _unitOfWork

// ✅ 正确 - 统一通过 UnitOfWork 访问
public class UserService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<User?> GetAsync(Guid id) => 
        await _unitOfWork.User.GetAsync(id);
}
```

### 缺少异步
```csharp
// ❌ 错误
public User GetById(Guid id) => _repo.GetById(id);

// ✅ 正确
public async Task<User> GetByIdAsync(Guid id) => await _repo.GetByIdAsync(id);
```

### 对象映射问题
```csharp
// ❌ 错误 - 注入 IMapper
public class RoleService
{
    private readonly IMapper _mapper;
    
    public RoleDto GetRole(Guid id)
    {
        var role = _unitOfWork.Role.GetAsync(id);
        return _mapper.Map<RoleDto>(role);  // 依赖注入
    }
}

// ❌ 错误 - 手写属性赋值
var dto = new RoleDto
{
    Id = role.Id,
    Name = role.Name,
    Description = role.Description  // 容易遗漏属性
};

// ✅ 正确 - 使用静态 Mapper 扩展方法
public class RoleService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<RoleDto?> GetRoleAsync(Guid id)
    {
        var role = await _unitOfWork.Role.GetAsync(id);
        return role?.ToDto();  // 使用扩展方法
    }
    
    public async Task UpdateAsync(UpdateRoleDto dto)
    {
        var role = await _unitOfWork.Role.GetAsync(dto.Id);
        role.ApplyUpdate(dto);  // 使用 ApplyUpdate 更新实体
        await _unitOfWork.Role.UpdateAsync(role);
    }
}
```

### 硬编码字符串问题
```csharp
// ❌ 错误 - 硬编码 Action 和 EntityType
await _unitOfWork.AuditLog.InsertAsync(new AuditLog
{
    Action = "Create",
    EntityType = "Role"
});

// ✅ 正确 - 使用常量类
await _unitOfWork.AuditLog.InsertAsync(
    AuditLogMappers.CreateAuditLog(
        AuditConstants.AuditAction.Create,
        AuditConstants.AuditEntityType.Role,
        roleId,
        roleName
    )
);
```

### 硬编码敏感信息
```csharp
// ❌ 错误
var token = "sk-xxxxx";

// ✅ 正确
var token = _configuration["ApiKey"];
```

### 单元测试 Mock 问题
```csharp
// ❌ 错误 - 直接注入 Repository Mock
public UserServiceTests()
{
    _userRepositoryMock = new Mock<IUserRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    
    _userService = new UserService(
        _userRepositoryMock.Object,  // 不应直接注入
        _unitOfWorkMock.Object
    );
}

// ✅ 正确 - 通过 UnitOfWork 设置 Repository Mock
public UserServiceTests()
{
    _userRepositoryMock = new Mock<IUserRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    
    // 通过 UnitOfWork 访问 Repository
    _unitOfWorkMock.Setup(x => x.User).Returns(_userRepositoryMock.Object);
    
    _userService = new UserService(_unitOfWorkMock.Object);
}
```

## Vue/TypeScript 常见问题检查

### 缺少类型
```typescript
// ❌ 错误
const data = ref([]);

// ✅ 正确
const data = ref<UserInfo[]>([]);
```

### 缺少错误处理
```typescript
// ❌ 错误
const fetchData = async () => {
  const result = await getList();
  data.value = result.items;
};

// ✅ 正确
const fetchData = async () => {
  loading.value = true;
  try {
    const result = await getList();
    data.value = result.items;
  } catch (error) {
    Message.error('加载失败');
  } finally {
    loading.value = false;
  }
};
```

### 缺少国际化
```typescript
// ❌ 错误
Message.success('删除成功');

// ✅ 正确
Message.success(t('common.deleteSuccess'));
```
