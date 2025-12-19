# 开发需求 Prompt 模板

> 使用说明：复制对应模板，填写具体内容后发送给 AI 助手

---

## 项目规范

### 数据库规范
- 所有表必须包含 `timestamp` 字段（用于分页排序）
- 主键使用 `varchar(36)` UUID 格式
- 时间字段使用 `datetime`
- 布尔字段使用 `tinyint(1)`

### 后端规范
- **禁止手写 SQL**，全部使用 `Dawning.Shared.Dapper.Contrib` 实现
- 遵循 DDD 分层架构：Api → Application → Domain → Infra.Data
- Repository 继承 `RepositoryBase<TEntity>`
- 使用 `QueryBuilder` 构建查询条件

---

## 一、新增管理页面（完整版）

```markdown
## 功能需求
新增 [XXX] 管理页面，支持增删改查功能

## 技术栈
- 前端：Vue 3 + Arco Design + TypeScript
- 后端：.NET 8 + DDD架构 + Dawning.Shared.Dapper.Contrib（禁止手写SQL）
- 数据库：MySQL

## 涉及范围
- [x] 前后端都需要
- [x] 需要数据库变更（新建表 + 迁移脚本）

## 数据库字段
| 字段名 | 类型 | 说明 | 是否必填 |
|--------|------|------|----------|
| id | varchar(36) | 主键UUID | 是 |
| name | varchar(100) | 名称 | 是 |
| display_name | varchar(200) | 显示名称 | 否 |
| description | text | 描述 | 否 |
| is_active | tinyint(1) | 是否启用 | 是 |
| created_at | datetime | 创建时间 | 是 |
| updated_at | datetime | 更新时间 | 是 |
| timestamp | bigint | 时间戳（分页排序用） | 是 |

## 前端功能要求
参考 user/index.vue 实现以下功能：

### 搜索区
- [ ] 名称搜索（模糊查询）
- [ ] 状态筛选（启用/禁用）
- [ ] 查询/重置按钮

### 操作按钮
- [ ] 新增按钮
- [ ] 导出按钮（CSV/Excel）

### 数据表格
- [ ] 分页展示（基于 timestamp 排序）
- [ ] 多选支持
- [ ] 状态标签显示

### 行操作
- [ ] 查看详情
- [ ] 编辑
- [ ] 删除（二次确认）
- [ ] 更多操作下拉菜单（可选）

### 批量操作（可选）
- [ ] 批量启用
- [ ] 批量禁用
- [ ] 批量删除

### 弹窗表单
- [ ] 新增/编辑共用弹窗
- [ ] 表单验证
- [ ] 防重复提交

## 后端功能要求
### API 接口
- GET    /api/xxx          - 分页查询
- GET    /api/xxx/{id}     - 获取详情
- POST   /api/xxx          - 新增
- PUT    /api/xxx/{id}     - 更新
- DELETE /api/xxx/{id}     - 删除

### DDD 分层（使用 Dapper.Contrib，禁止手写SQL）
- Api: XxxController
- Application: XxxService, IXxxService
- Domain: XxxEntity, IXxxRepository
- Infra.Data: XxxRepository（继承 RepositoryBase，使用 QueryBuilder）

## 特殊说明
- [ ] 需要国际化支持
- [ ] 需要权限控制
- [ ] 需要审计日志
- [ ] 需要软删除
```

---

## 二、新增管理页面（简化版）

```markdown
## 新增 [XXX] 管理页面

参考 user/index.vue 实现，包含：
1. 搜索区：[字段1]、[字段2] 搜索
2. 表格：分页（基于timestamp排序）、多选、状态标签
3. 操作：新增、编辑、删除、查看
4. 弹窗：新增/编辑表单，防重复提交

数据库字段：
- name (varchar 100, 必填)
- description (text, 可选)
- is_active (boolean, 必填)
- timestamp (bigint, 必填，分页排序用)

后端 API：增删改查 + 分页
技术约束：禁止手写SQL，使用 Dawning.Shared.Dapper.Contrib
```

---

## 三、修改现有功能

```markdown
## 功能需求
在 [页面名称] 页面添加 [XXX] 功能

## 涉及范围
- [ ] 仅前端
- [ ] 仅后端
- [ ] 前后端都需要
- [ ] 需要数据库变更（如需，确保包含 timestamp 字段）

## 详细要求
1. [具体要求1]
2. [具体要求2]
3. [具体要求3]

## 技术约束
- 后端禁止手写SQL，使用 Dawning.Shared.Dapper.Contrib

## 参考页面/代码
- 参考 xxx/index.vue 的实现方式

## 特殊说明
- [如需国际化、权限控制等]
```

---

## 四、Bug修复/问题排查

```markdown
## 问题描述
[页面名称] 的 [具体问题]

## 复现步骤
1. 打开 xxx 页面
2. 点击 xxx
3. 出现 xxx 问题

## 期望行为
[描述正确的行为应该是什么]

## 错误信息（如有）
[粘贴控制台报错或后端日志]
```

---

## 五、代码优化/重构

```markdown
## 优化目标
对 [模块/页面名称] 进行优化

## 当前问题
1. [问题1：如代码重复]
2. [问题2：如性能问题]
3. [问题3：如可读性差]

## 期望结果
1. [期望1]
2. [期望2]

## 约束条件
- [ ] 不能破坏现有功能
- [ ] 需要保持向后兼容
- [ ] 需要添加单元测试
- [ ] 后端禁止手写SQL，使用 Dawning.Shared.Dapper.Contrib
```

---

## 六、批量操作

```markdown
## 批量操作需求
对所有 [XXX] 页面/文件 执行以下操作：

## 操作内容
1. [操作1：如添加防重复提交]
2. [操作2：如统一样式]
3. [操作3：如添加国际化]

## 涉及文件范围
- [ ] 所有 views 下的 .vue 文件
- [ ] 所有 api 下的 .ts 文件
- [ ] 指定目录：xxx/

## 技术约束
- 后端禁止手写SQL，使用 Dawning.Shared.Dapper.Contrib
- 数据库表必须包含 timestamp 字段用于分页排序

## 参考示例
- 参考 xxx/index.vue 的实现方式
```

---

## 提示

1. **信息越具体，开发越准确** - 提供字段名、类型、验证规则等细节
2. **提供参考页面** - 直接说明参考哪个现有页面的实现
3. **明确涉及范围** - 前端/后端/数据库，避免遗漏
4. **特殊要求提前说明** - 国际化、权限、审计日志等
5. **技术约束** - 后端禁止手写SQL，使用 Dapper.Contrib；数据库必须有 timestamp 字段
