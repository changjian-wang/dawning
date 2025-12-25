-- 创建用户表
CREATE TABLE IF NOT EXISTS `users` (
    `id` CHAR(36) NOT NULL COMMENT '用户唯一标识(GUID)',
    `username` VARCHAR(50) NOT NULL COMMENT '用户名（登录名）',
    `password_hash` VARCHAR(255) NOT NULL COMMENT '密码哈希',
    `email` VARCHAR(100) NULL COMMENT '邮箱',
    `phone_number` VARCHAR(20) NULL COMMENT '手机号',
    `display_name` VARCHAR(100) NULL COMMENT '显示名称',
    `avatar` VARCHAR(500) NULL COMMENT '头像URL',
    `role` VARCHAR(50) NOT NULL DEFAULT 'user' COMMENT '角色（admin, user, manager等）',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否激活',
    `is_deleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否已删除（软删除）',
    `email_confirmed` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '邮箱是否已验证',
    `phone_number_confirmed` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '手机号是否已验证',
    `last_login_at` DATETIME NULL COMMENT '最后登录时间',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    `updated_at` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    `created_by` CHAR(36) NULL COMMENT '创建者ID',
    `updated_by` CHAR(36) NULL COMMENT '更新者ID',
    `remark` VARCHAR(500) NULL COMMENT '备注',
    PRIMARY KEY (`id`),
    UNIQUE INDEX `uk_username` (`username` ASC),
    UNIQUE INDEX `uk_email` (`email` ASC),
    INDEX `idx_role` (`role` ASC),
    INDEX `idx_is_active` (`is_active` ASC),
    INDEX `idx_is_deleted` (`is_deleted` ASC),
    INDEX `idx_created_at` (`created_at` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='用户表';

-- 插入初始管理员用户（密码: Admin@123）
INSERT INTO `users` (
    `id`,
    `username`,
    `password_hash`,
    `email`,
    `display_name`,
    `role`,
    `is_active`,
    `created_at`
) VALUES (
    UUID(),
    'admin',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=', -- PBKDF2 hash of 'Admin@123'
    'admin@dawning.com',
    'Administrator',
    'admin',
    1,
    UTC_TIMESTAMP()
);

-- 插入测试普通用户（密码: Admin@123）
INSERT INTO `users` (
    `id`,
    `username`,
    `password_hash`,
    `email`,
    `display_name`,
    `role`,
    `is_active`,
    `created_at`
) VALUES (
    UUID(),
    'user',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=', -- PBKDF2 hash of 'Admin@123'
    'user@dawning.com',
    'Test User',
    'user',
    1,
    UTC_TIMESTAMP()
);
