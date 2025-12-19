-- ================================================
-- V12: 删除所有外键约束
-- 文件: V12_remove_foreign_keys.sql
-- 日期: 2025-12-19
-- 说明: 删除所有外键约束以提升SQL性能
--       外键约束会在INSERT/UPDATE/DELETE时进行额外检查，影响性能
--       数据完整性由应用层保证
-- ================================================

USE dawning_identity;

-- ================================================
-- 1. OpenIddict 表外键
-- ================================================

-- openiddict_authorizations 表
ALTER TABLE `openiddict_authorizations` 
    DROP FOREIGN KEY IF EXISTS `fk_authorization_application`;

-- openiddict_tokens 表
ALTER TABLE `openiddict_tokens` 
    DROP FOREIGN KEY IF EXISTS `fk_token_application`,
    DROP FOREIGN KEY IF EXISTS `fk_token_authorization`;

-- ================================================
-- 2. 用户角色关联表外键
-- ================================================

-- user_roles 表
ALTER TABLE `user_roles` 
    DROP FOREIGN KEY IF EXISTS `fk_user_roles_user`,
    DROP FOREIGN KEY IF EXISTS `fk_user_roles_role`;

-- ================================================
-- 3. 角色权限关联表外键
-- ================================================

-- role_permissions 表
ALTER TABLE `role_permissions` 
    DROP FOREIGN KEY IF EXISTS `fk_role_permissions_role`,
    DROP FOREIGN KEY IF EXISTS `fk_role_permissions_permission`;

-- ================================================
-- 4. API资源相关表外键
-- ================================================

-- api_resource_scopes 表
ALTER TABLE `api_resource_scopes` 
    DROP FOREIGN KEY IF EXISTS `fk_api_resource_scopes_resource`;

-- api_resource_claims 表
ALTER TABLE `api_resource_claims` 
    DROP FOREIGN KEY IF EXISTS `fk_api_resource_claims_resource`;

-- ================================================
-- 5. Identity资源相关表外键
-- ================================================

-- identity_resource_claims 表
ALTER TABLE `identity_resource_claims` 
    DROP FOREIGN KEY IF EXISTS `fk_identity_resource_claims_resource`;

-- ================================================
-- 6. 网关路由表外键
-- ================================================

-- gateway_routes 表
ALTER TABLE `gateway_routes` 
    DROP FOREIGN KEY IF EXISTS `fk_gateway_routes_cluster`;

-- ================================================
-- 验证外键已删除
-- ================================================

SELECT '正在验证外键删除...' AS message;

-- 检查是否还有外键约束
SELECT 
    TABLE_NAME,
    CONSTRAINT_NAME,
    REFERENCED_TABLE_NAME
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'dawning_identity'
  AND REFERENCED_TABLE_NAME IS NOT NULL;

SELECT '外键删除完成！' AS message;
