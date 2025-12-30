-- Create users table
CREATE TABLE IF NOT EXISTS `users` (
    `id` CHAR(36) NOT NULL COMMENT 'User unique identifier (GUID)',
    `username` VARCHAR(50) NOT NULL COMMENT 'Username (login name)',
    `password_hash` VARCHAR(255) NOT NULL COMMENT 'Password hash',
    `email` VARCHAR(100) NULL COMMENT 'Email',
    `phone_number` VARCHAR(20) NULL COMMENT 'Phone number',
    `display_name` VARCHAR(100) NULL COMMENT 'Display name',
    `avatar` VARCHAR(500) NULL COMMENT 'Avatar URL',
    `role` VARCHAR(50) NOT NULL DEFAULT 'user' COMMENT 'Role (admin, user, manager, etc.)',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Is active',
    `is_system` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Is system user (system users cannot be deleted/disabled)',
    `is_deleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Is deleted (soft delete)',
    `email_confirmed` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Email confirmed',
    `phone_number_confirmed` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Phone confirmed',
    `last_login_at` DATETIME NULL COMMENT 'Last login time',
    `failed_login_count` INT NOT NULL DEFAULT 0 COMMENT 'Failed login count',
    `lockout_end` DATETIME NULL COMMENT 'Lockout end time',
    `lockout_enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Lockout enabled',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Created time',
    `updated_at` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Updated time',
    `created_by` CHAR(36) NULL COMMENT 'Creator ID',
    `updated_by` CHAR(36) NULL COMMENT 'Updater ID',
    `remark` VARCHAR(500) NULL COMMENT 'Remark',
    `timestamp` BIGINT NOT NULL DEFAULT 0 COMMENT 'Timestamp in milliseconds (for pagination)',
    PRIMARY KEY (`id`),
    UNIQUE INDEX `uk_username` (`username` ASC),
    UNIQUE INDEX `uk_email` (`email` ASC),
    INDEX `idx_role` (`role` ASC),
    INDEX `idx_is_active` (`is_active` ASC),
    INDEX `idx_is_system` (`is_system` ASC),
    INDEX `idx_is_deleted` (`is_deleted` ASC),
    INDEX `idx_created_at` (`created_at` DESC),
    INDEX `idx_timestamp` (`timestamp` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Users table';

-- Insert initial admin user (password: Admin@123)
INSERT INTO `users` (
    `id`,
    `username`,
    `password_hash`,
    `email`,
    `display_name`,
    `role`,
    `is_active`,
    `is_system`,
    `timestamp`,
    `created_at`
) VALUES (
    UUID(),
    'admin',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=', -- PBKDF2 hash of 'Admin@123'
    'admin@dawning.com',
    'Administrator',
    'admin',
    1,
    1,
    UNIX_TIMESTAMP() * 1000,
    UTC_TIMESTAMP()
);

-- Insert test user (password: Admin@123)
INSERT INTO `users` (
    `id`,
    `username`,
    `password_hash`,
    `email`,
    `display_name`,
    `role`,
    `is_active`,
    `is_system`,
    `timestamp`,
    `created_at`
) VALUES (
    UUID(),
    'user',
    '100000;HQ2Qh1DuRQGvMRyDJxW23Q==;kgr3tG5/7WTyN/xjxDdWdhRJW740p0kwlcHKnFfIoMc=', -- PBKDF2 hash of 'Admin@123'
    'user@dawning.com',
    'Test User',
    'user',
    1,
    0,
    UNIX_TIMESTAMP() * 1000,
    UTC_TIMESTAMP()
);
