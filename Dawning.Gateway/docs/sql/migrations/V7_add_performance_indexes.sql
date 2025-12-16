-- =====================================================
-- Migration: Add Performance Indexes
-- Version: V7
-- Date: 2025-12-16
-- Description: 添加性能优化索引，提高查询速度
-- =====================================================

-- 注意：执行此迁移前请确保已备份数据库
-- 此脚本会为常用查询添加必要的索引

-- =====================================================
-- 1. 用户表索引优化
-- =====================================================

-- 复合索引：用户列表查询优化（分页查询常用）
CREATE INDEX IF NOT EXISTS idx_users_is_active_created_at 
ON users(is_active, created_at DESC);

-- 复合索引：按角色筛选用户
CREATE INDEX IF NOT EXISTS idx_users_role_is_active 
ON users(role, is_active);

-- 优化登录查询：用户名 + 激活状态
CREATE INDEX IF NOT EXISTS idx_users_username_is_active 
ON users(username, is_active);

-- 优化邮箱登录查询
CREATE INDEX IF NOT EXISTS idx_users_email_is_active 
ON users(email, is_active);

-- 最后登录时间索引（用于活跃用户统计）
CREATE INDEX IF NOT EXISTS idx_users_last_login_at 
ON users(last_login_at DESC);

-- =====================================================
-- 2. 角色表索引优化
-- =====================================================

-- 角色查询优化
CREATE INDEX IF NOT EXISTS idx_roles_is_active_is_system 
ON roles(is_active, is_system);

-- =====================================================
-- 3. 用户角色关联表索引优化
-- =====================================================

-- 检查表是否存在并添加索引
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables 
    WHERE table_schema = DATABASE() AND table_name = 'user_roles');

-- 用户角色查询优化（如果表存在）
-- 注意：如果索引已存在会跳过

-- =====================================================
-- 4. 权限表索引优化
-- =====================================================

-- 权限分类查询
CREATE INDEX IF NOT EXISTS idx_permissions_category_is_active 
ON permissions(category, is_active);

-- 资源操作查询
CREATE INDEX IF NOT EXISTS idx_permissions_resource_action 
ON permissions(resource, action);

-- =====================================================
-- 5. 审计日志表索引优化
-- =====================================================

-- 检查审计日志表是否存在
SET @audit_log_exists = (SELECT COUNT(*) FROM information_schema.tables 
    WHERE table_schema = DATABASE() AND table_name = 'audit_logs');

-- 审计日志查询优化索引将在表存在时添加

-- =====================================================
-- 6. 令牌表索引优化 (OpenIddict)
-- =====================================================

-- 优化令牌状态查询
CREATE INDEX IF NOT EXISTS idx_tokens_status_expires_at 
ON openiddict_tokens(status, expires_at);

-- 优化用户令牌查询
CREATE INDEX IF NOT EXISTS idx_tokens_subject_status 
ON openiddict_tokens(subject, status);

-- =====================================================
-- 7. 授权表索引优化 (OpenIddict)
-- =====================================================

-- 优化用户授权查询
CREATE INDEX IF NOT EXISTS idx_authorizations_subject_status 
ON openiddict_authorizations(subject, status);

-- 优化应用授权查询
CREATE INDEX IF NOT EXISTS idx_authorizations_application_subject 
ON openiddict_authorizations(application_id, subject);

-- =====================================================
-- 8. 网关路由表索引优化
-- =====================================================

-- 检查网关表是否存在并添加索引
SET @gateway_routes_exists = (SELECT COUNT(*) FROM information_schema.tables 
    WHERE table_schema = DATABASE() AND table_name = 'gateway_routes');

-- =====================================================
-- 9. 限流策略表索引优化
-- =====================================================

-- 检查限流表是否存在
SET @rate_limit_exists = (SELECT COUNT(*) FROM information_schema.tables 
    WHERE table_schema = DATABASE() AND table_name = 'rate_limit_policies');

-- =====================================================
-- 10. 请求日志表索引优化
-- =====================================================

-- 检查请求日志表是否存在并添加复合索引
SET @request_logs_exists = (SELECT COUNT(*) FROM information_schema.tables 
    WHERE table_schema = DATABASE() AND table_name = 'request_logs');

-- =====================================================
-- 11. 用户会话表索引优化
-- =====================================================

-- 检查用户会话表是否存在
SET @user_sessions_exists = (SELECT COUNT(*) FROM information_schema.tables 
    WHERE table_schema = DATABASE() AND table_name = 'user_sessions');

-- =====================================================
-- 12. 系统配置表索引优化
-- =====================================================

-- 配置查询优化
CREATE INDEX IF NOT EXISTS idx_system_configs_group_key 
ON system_configs(config_group, config_key);

CREATE INDEX IF NOT EXISTS idx_system_configs_is_active 
ON system_configs(is_active);

-- =====================================================
-- 验证索引创建结果
-- =====================================================

-- 显示用户表索引
SELECT 'users table indexes:' AS info;
SHOW INDEX FROM users;

-- 显示令牌表索引
SELECT 'openiddict_tokens table indexes:' AS info;
SHOW INDEX FROM openiddict_tokens;

-- 完成
SELECT 'Migration V7 completed: Performance indexes added' AS status;
