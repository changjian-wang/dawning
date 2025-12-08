
> DROP TABLE IF EXISTS `openiddict_applications`;
  
> CREATE TABLE `openiddict_applications` (
      `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
      `client_id` VARCHAR(100) NOT NULL COMMENT '客户端ID',
      `client_secret` VARCHAR(500) NULL COMMENT '客户端密钥（哈希后）',
      `display_name` VARCHAR(200) NULL COMMENT '显示名称',
      `type` VARCHAR(50) NULL COMMENT '客户端类型（confidential, public）',
      `consent_type` VARCHAR(50) NULL COMMENT '同意类型（explicit, implicit, systematic）',
      `permissions` TEXT NULL COMMENT '权限列表（JSON格式）',
      `redirect_uris` TEXT NULL COMMENT '重定向URI列表（JSON格式）',
      `post_logout_redirect_uris` TEXT NULL COMMENT '登出后重定向URI列表（JSON格式）',
      `requirements` TEXT NULL COMMENT '要求列表（JSON格式）',
> DROP TABLE IF EXISTS `openiddict_scopes`;
  
> CREATE TABLE `openiddict_scopes` (
      `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
      `name` VARCHAR(200) NOT NULL COMMENT '作用域名称',
      `display_name` VARCHAR(200) NULL COMMENT '显示名称',
      `description` VARCHAR(500) NULL COMMENT '描述',
      `resources` TEXT NULL COMMENT '资源列表（JSON格式）',
      `properties` TEXT NULL COMMENT '扩展属性（JSON格式）',
      `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳-毫秒（UTC，用于索引和分页）',
      `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
      PRIMARY KEY (`id`),
      UNIQUE KEY `uk_name` (`name`),
> DROP TABLE IF EXISTS `openiddict_authorizations`;
  
> CREATE TABLE `openiddict_authorizations` (
      `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
      `application_id` CHAR(36) NULL COMMENT '关联的应用程序ID',
      `subject` VARCHAR(200) NULL COMMENT '用户标识',
      `type` VARCHAR(50) NULL COMMENT '授权类型',
      `status` VARCHAR(50) NULL COMMENT '授权状态（valid, revoked）',
      `scopes` TEXT NULL COMMENT '授权的作用域列表（JSON格式）',
      `properties` TEXT NULL COMMENT '扩展属性（JSON格式）',
      `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳-毫秒（UTC，用于索引和分页）',
      `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
      PRIMARY KEY (`id`),
> DROP TABLE IF EXISTS `openiddict_tokens`;
  
> CREATE TABLE `openiddict_tokens` (
      `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
      `application_id` CHAR(36) NULL COMMENT '关联的应用程序ID',
      `authorization_id` CHAR(36) NULL COMMENT '关联的授权ID',
      `subject` VARCHAR(200) NULL COMMENT '用户标识',
      `type` VARCHAR(50) NULL COMMENT '令牌类型（access_token, refresh_token, id_token）',
      `status` VARCHAR(50) NULL COMMENT '令牌状态（valid, revoked, redeemed）',
      `payload` TEXT NULL COMMENT '令牌负载（JWT）',
      `reference_id` VARCHAR(200) NULL COMMENT '引用ID（用于令牌内省）',
      `expires_at` DATETIME NULL COMMENT '过期时间（UTC）',
      `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳-毫秒（UTC，用于索引和分页）',
> INSERT INTO `openiddict_applications` (
      `id`, `client_id`, `client_secret`, `display_name`, `type`, `consent_type`,
      `permissions`, `redirect_uris`, `post_logout_redirect_uris`, 
      `requirements`, `properties`, `timestamp`, `created_at`
  ) VALUES (
      UUID(),
      'test-client',
      'test-secret-hash',
      'Test Application',
      'confidential',
      'explicit',
> INSERT INTO `openiddict_scopes` (
      `id`, `name`, `display_name`, `description`, 
      `resources`, `properties`, `timestamp`, `created_at`
  ) VALUES 
  (UUID(), 'openid', 'OpenID', 'OpenID Connect scope', '[]', '{}', UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()),
  (UUID(), 'profile', 'Profile', 'User profile information', '[]', '{}', UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()),
  (UUID(), 'email', 'Email', 'User email address', '[]', '{}', UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()),
  (UUID(), 'api', 'API', 'API access scope', '["api-resource"]', '{}', UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP());
  
  -- ====================================================
  -- 清理过期令牌的存储过程

