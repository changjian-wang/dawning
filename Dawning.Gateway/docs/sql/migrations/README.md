# 数据库迁移脚本

本目录包含 Dawning Gateway 系统的增量数据库迁移脚本。

## 迁移脚本列表

| 版本 | 文件名 | 描述 | 日期 |
|------|--------|------|------|
| V2 | V2_remove_soft_delete.sql | 移除软删除字段，简化数据模型 | 2024-12-15 |
| V3 | V3_add_login_lockout.sql | 添加登录锁定功能和密码策略配置 | 2024-12-16 |

## 执行顺序

1. **首次部署**：先执行主表创建脚本（`dawning_identity.sql`、`gateway_tables.sql` 等）
2. **增量迁移**：按版本号顺序执行 migrations 目录下的脚本

## 执行方式

### 使用 MySQL 命令行

```bash
# 连接数据库
mysql -u root -p dawning_identity

# 执行迁移脚本
source migrations/V2_remove_soft_delete.sql
source migrations/V3_add_login_lockout.sql
```

### 使用 Docker

```bash
# 在 docker-compose 环境中执行
docker-compose exec mysql mysql -u root -p dawning_identity < migrations/V3_add_login_lockout.sql
```

## 迁移前注意事项

1. **备份数据库**：执行任何迁移前，请先备份数据库
2. **检查版本**：确保按顺序执行，不要跳过中间版本
3. **测试环境**：建议先在测试环境验证迁移脚本

## 回滚

部分迁移脚本可能包含回滚语句（注释状态），如需回滚请谨慎操作：

```sql
-- 回滚示例（V3）
-- ALTER TABLE `users` DROP COLUMN `failed_login_count`;
-- ALTER TABLE `users` DROP COLUMN `lockout_end`;
-- ALTER TABLE `users` DROP COLUMN `lockout_enabled`;
-- DROP INDEX `idx_users_lockout` ON `users`;
```

## 迁移脚本开发规范

1. 文件命名：`V{版本号}_{描述}.sql`（如 `V4_add_new_feature.sql`）
2. 包含版本信息头注释
3. 使用事务保证原子性（如适用）
4. 提供回滚脚本（作为注释）
5. 更新本 README 文件
