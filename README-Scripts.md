# Dawning 开发脚本

## 快速开始

### 构建项目
```powershell
.\build.ps1
```

### 运行后端
```powershell
.\run.ps1          # 直接运行
.\run.ps1 -Clean   # 清理后运行
```

### 运行测试
```powershell
.\test.ps1
```

## 当前状态

### ✅ 已完成
1. **移除软删除功能** - 所有 IsDeleted 字段和软删除逻辑已从后端移除
2. **移除 IdentityServer4** - 前端已完全重写，移除所有 IS4 依赖
3. **OpenIddict集成** - 后端使用 OpenIddict 进行认证
4. **JSON序列化** - 配置为 camelCase，前后端统一
5. **重置密码API** - 添加管理员重置用户密码功能
6. **集成测试框架** - 使用 WebApplicationFactory 创建测试项目

### ⚠️ 待修复
1. **测试数据库** - `dawning_identity_test` 数据库缺少表结构
2. **测试认证** - 集成测试需要获取认证token才能调用受保护的API
3. **SQL文件** - 需要将 `dawning_identity.sql` 导入到测试数据库

## 修复步骤

### 1. 导入数据库结构
```powershell
mysql -h localhost -u aluneth -p123456 dawning_identity_test < .\Dawning.Gateway\docs\sql\dawning_identity.sql
```

### 2. 禁用测试认证（临时方案）
在 `TestWebApplicationFactory.cs` 中禁用认证：
```csharp
builder.ConfigureTestServices(services =>
{
    services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => {});
});
```

## 数据库配置

- **开发数据库**: `dawning_identity`
- **测试数据库**: `dawning_identity_test`
- **连接信息**: localhost:3306, 用户: aluneth, 密码: 123456

## API端口

- **后端API**: http://localhost:5202
- **Swagger**: http://localhost:5202/swagger

## 项目结构

```
Dawning.Gateway/
├── src/
│   ├── Dawning.Identity.Api/          # API 主项目
│   ├── Dawning.Identity.Application/  # 应用层
│   ├── Dawning.Identity.Domain/       # 领域层
│   ├── Dawning.Identity.Infra.Data/   # 数据访问层
│   └── Dawning.Identity.Api.Tests/    # 集成测试项目
└── docs/
    └── sql/
        └── dawning_identity.sql       # 数据库脚本
```

## 开发注意事项

1. **表名**: 数据库表名是 `users` (复数)，不是 `user`
2. **软删除已移除**: DeleteAsync 现在是硬删除
3. **camelCase**: API返回的JSON字段使用 camelCase (username, displayName, isActive)
4. **前端重写**: user.ts 和 index.vue 已完全重写，移除所有 IS4 字段

## 下次继续的任务

1. 修复测试数据库表结构
2. 实现测试认证机制或禁用认证
3. 运行并通过所有集成测试
4. 测试前端功能
5. 创建前端构建和运行脚本
