-- 用户数据清理和初始化脚本
-- 创建时间: 2025-12-09
-- 用途: 清理用户表，初始化管理员账户，添加测试数据

USE dawning_identity;

-- 1. 清空现有用户数据
TRUNCATE TABLE users;

-- 2. 初始化管理员账户
-- 密码: Admin@123 (使用 PBKDF2 哈希: 100000;salt;hash 格式)
INSERT INTO users (
    id,
    username,
    password_hash,
    display_name,
    email,
    phone_number,
    role,
    is_active,
    email_confirmed,
    phone_number_confirmed,
    timestamp,
    created_at
) VALUES (
    UUID(),
    'admin',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=', -- 示例哈希值
    '系统管理员',
    'admin@dawning.com',
    '13800138000',
    'admin',
    1,
    1,
    1,
    UNIX_TIMESTAMP(),
    NOW()
);

-- 3. 添加测试用户数据
INSERT INTO users (
    id,
    username,
    password_hash,
    display_name,
    email,
    phone_number,
    role,
    is_active,
    email_confirmed,
    phone_number_confirmed,
    timestamp,
    created_at
) VALUES 
(
    UUID(),
    'zhangsan',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=',
    '张三',
    'zhangsan@dawning.com',
    '13800138001',
    'user',
    1,
    1,
    1,
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'lisi',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=',
    '李四',
    'lisi@dawning.com',
    '13800138002',
    'user',
    1,
    1,
    0,
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'wangwu',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=',
    '王五',
    'wangwu@dawning.com',
    '13800138003',
    'manager',
    1,
    0,
    1,
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'zhaoliu',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=',
    '赵六',
    'zhaoliu@dawning.com',
    '13800138004',
    'user',
    0,
    1,
    1,
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'sunqi',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=',
    '孙七',
    'sunqi@dawning.com',
    '13800138005',
    'user',
    1,
    1,
    1,
    UNIX_TIMESTAMP(),
    NOW()
);

-- 查看插入结果
SELECT 
    username AS '用户名',
    display_name AS '显示名称',
    email AS '邮箱',
    phone_number AS '手机号',
    role AS '角色',
    is_active AS '账户状态',
    email_confirmed AS '邮箱已确认',
    phone_number_confirmed AS '手机已确认'
FROM users
ORDER BY created_at;
