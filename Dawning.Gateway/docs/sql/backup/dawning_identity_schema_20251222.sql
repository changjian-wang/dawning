mysqldump: [Warning] Using a password on the command line interface can be insecure.
-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: dawning_identity
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `alert_history`
--

DROP TABLE IF EXISTS `alert_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `alert_history` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `rule_id` bigint unsigned NOT NULL COMMENT '鍛婅瑙勫垯ID',
  `rule_name` varchar(100) COLLATE utf8mb4_general_ci NOT NULL COMMENT '瑙勫垯鍚嶇О',
  `metric_type` varchar(50) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鎸囨爣绫诲瀷',
  `metric_value` decimal(18,4) NOT NULL COMMENT '瑙﹀彂鏃剁殑鎸囨爣鍊?,
  `threshold` decimal(18,4) NOT NULL COMMENT '闃堝€?,
  `severity` varchar(20) COLLATE utf8mb4_general_ci NOT NULL COMMENT '涓ラ噸绋嬪害',
  `message` varchar(1000) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鍛婅娑堟伅',
  `status` varchar(20) COLLATE utf8mb4_general_ci NOT NULL DEFAULT 'triggered' COMMENT '鐘舵€? triggered, acknowledged, resolved',
  `triggered_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '瑙﹀彂鏃堕棿',
  `acknowledged_at` datetime DEFAULT NULL COMMENT '纭鏃堕棿',
  `acknowledged_by` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '纭浜?,
  `resolved_at` datetime DEFAULT NULL COMMENT '瑙ｅ喅鏃堕棿',
  `resolved_by` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '瑙ｅ喅浜?,
  `notify_sent` tinyint(1) NOT NULL DEFAULT '0' COMMENT '鏄惁宸插彂閫侀€氱煡',
  `notify_result` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '閫氱煡鍙戦€佺粨鏋?,
  PRIMARY KEY (`id`),
  KEY `idx_alert_history_rule_id` (`rule_id`),
  KEY `idx_alert_history_status` (`status`),
  KEY `idx_alert_history_severity` (`severity`),
  KEY `idx_alert_history_triggered_at` (`triggered_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='鍛婅鍘嗗彶琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `alert_rules`
--

DROP TABLE IF EXISTS `alert_rules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `alert_rules` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(100) COLLATE utf8mb4_general_ci NOT NULL COMMENT '瑙勫垯鍚嶇О',
  `description` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '瑙勫垯鎻忚堪',
  `metric_type` varchar(50) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鎸囨爣绫诲瀷: cpu, memory, response_time, error_rate, request_count',
  `operator` varchar(20) COLLATE utf8mb4_general_ci NOT NULL COMMENT '姣旇緝鎿嶄綔绗? gt, gte, lt, lte, eq',
  `threshold` decimal(18,4) NOT NULL COMMENT '闃堝€?,
  `duration_seconds` int NOT NULL DEFAULT '60' COMMENT '鎸佺画鏃堕棿(绉?',
  `severity` varchar(20) COLLATE utf8mb4_general_ci NOT NULL DEFAULT 'warning' COMMENT '涓ラ噸绋嬪害: info, warning, error, critical',
  `is_enabled` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鏄惁鍚敤',
  `notify_channels` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '閫氱煡娓犻亾',
  `notify_emails` varchar(1000) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '閫氱煡閭',
  `webhook_url` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'Webhook URL',
  `cooldown_minutes` int NOT NULL DEFAULT '5' COMMENT '鍐峰嵈鏃堕棿(鍒嗛挓)',
  `last_triggered_at` datetime DEFAULT NULL COMMENT '涓婃瑙﹀彂鏃堕棿',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_alert_rules_metric_type` (`metric_type`),
  KEY `idx_alert_rules_is_enabled` (`is_enabled`),
  KEY `idx_alert_rules_severity` (`severity`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='鍛婅瑙勫垯琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `api_resource_claims`
--

DROP TABLE IF EXISTS `api_resource_claims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `api_resource_claims` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鍞竴鏍囪瘑',
  `api_resource_id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT 'API璧勬簮ID',
  `type` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Claim绫诲瀷',
  `created_at` datetime NOT NULL COMMENT '鍒涘缓鏃堕棿',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_api_resource_claims` (`api_resource_id`,`type`),
  KEY `idx_api_resource_claims_resource_id` (`api_resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='API璧勬簮澹版槑琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `api_resource_scopes`
--

DROP TABLE IF EXISTS `api_resource_scopes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `api_resource_scopes` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鍞竴鏍囪瘑',
  `api_resource_id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT 'API璧勬簮ID',
  `scope` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Scope鍚嶇О',
  `created_at` datetime NOT NULL COMMENT '鍒涘缓鏃堕棿',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_api_resource_scopes` (`api_resource_id`,`scope`),
  KEY `idx_api_resource_scopes_resource_id` (`api_resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='API璧勬簮浣滅敤鍩熷叧鑱旇〃';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `api_resources`
--

DROP TABLE IF EXISTS `api_resources`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `api_resources` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT 'API璧勬簮鍞竴鏍囪瘑',
  `name` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT 'API璧勬簮鍚嶇О',
  `display_name` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鏄剧ず鍚嶇О',
  `description` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎻忚堪淇℃伅',
  `enabled` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鏄惁鍚敤',
  `allowed_access_token_signing_algorithms` text COLLATE utf8mb4_general_ci COMMENT '鍏佽鐨勮闂护鐗岀鍚嶇畻娉?JSON鏁扮粍)',
  `show_in_discovery_document` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鏄惁鍦ㄥ彂鐜版枃妗ｄ腑鏄剧ず',
  `properties` text COLLATE utf8mb4_general_ci COMMENT '鎵╁睍灞炴€?JSON瀵硅薄)',
  `timestamp` bigint NOT NULL COMMENT '鏃堕棿鎴?,
  `created_at` datetime NOT NULL COMMENT '鍒涘缓鏃堕棿',
  `updated_at` datetime DEFAULT NULL COMMENT '鏇存柊鏃堕棿',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_api_resources_name` (`name`),
  KEY `idx_api_resources_enabled` (`enabled`),
  KEY `idx_api_resources_created_at` (`created_at`),
  KEY `idx_timestamp` (`timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='API璧勬簮琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `audit_logs`
--

DROP TABLE IF EXISTS `audit_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `audit_logs` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '瀹¤鏃ュ織ID (GUID)',
  `user_id` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎿嶄綔鐢ㄦ埛ID',
  `username` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎿嶄綔鐢ㄦ埛鍚?,
  `action` varchar(100) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鎿嶄綔绫诲瀷',
  `entity_type` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '瀹炰綋绫诲瀷',
  `entity_id` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '瀹炰綋ID',
  `description` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎿嶄綔鎻忚堪',
  `ip_address` varchar(45) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'IP鍦板潃',
  `user_agent` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鐢ㄦ埛浠ｇ悊',
  `request_path` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '璇锋眰璺緞',
  `request_method` varchar(10) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '璇锋眰鏂规硶',
  `status_code` int DEFAULT NULL COMMENT 'HTTP鐘舵€佺爜',
  `old_values` json DEFAULT NULL COMMENT '淇敼鍓嶇殑鍊?,
  `new_values` json DEFAULT NULL COMMENT '淇敼鍚庣殑鍊?,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '鍒涘缓鏃堕棿',
  PRIMARY KEY (`id`),
  KEY `idx_audit_logs_user_id` (`user_id`),
  KEY `idx_audit_logs_action` (`action`),
  KEY `idx_audit_logs_entity_type` (`entity_type`),
  KEY `idx_audit_logs_entity_id` (`entity_id`),
  KEY `idx_audit_logs_created_at` (`created_at`),
  KEY `idx_audit_logs_ip_address` (`ip_address`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='瀹¤鏃ュ織琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `claim_types`
--

DROP TABLE IF EXISTS `claim_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `claim_types` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '涓婚敭 (GUID)',
  `name` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鍚嶇О',
  `display_name` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鏄剧ず鍚嶇О',
  `type` varchar(50) COLLATE utf8mb4_general_ci NOT NULL COMMENT '绫诲瀷',
  `description` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎻忚堪璇存槑',
  `required` tinyint(1) NOT NULL DEFAULT '0' COMMENT '鏄惁蹇呴』椤?,
  `non_editable` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鐢ㄦ埛鏄惁鍙紪杈?,
  `timestamp` bigint NOT NULL COMMENT 'Unix鏃堕棿鎴?姣',
  `created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '鍒涘缓鏃堕棿',
  `updated` datetime DEFAULT NULL COMMENT '鏇存柊鏃堕棿',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_name` (`name`),
  KEY `idx_type` (`type`),
  KEY `idx_timestamp` (`timestamp`),
  KEY `idx_created` (`created`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='澹版槑绫诲瀷琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gateway_clusters`
--

DROP TABLE IF EXISTS `gateway_clusters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gateway_clusters` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL DEFAULT (uuid()),
  `cluster_id` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `name` varchar(200) COLLATE utf8mb4_general_ci NOT NULL,
  `description` text COLLATE utf8mb4_general_ci,
  `load_balancing_policy` varchar(50) COLLATE utf8mb4_general_ci DEFAULT 'RoundRobin',
  `destinations` json NOT NULL,
  `health_check_enabled` tinyint(1) DEFAULT '0',
  `health_check_interval` int DEFAULT NULL,
  `health_check_timeout` int DEFAULT NULL,
  `health_check_path` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `passive_health_policy` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `session_affinity_enabled` tinyint(1) DEFAULT '0',
  `session_affinity_policy` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `session_affinity_key_name` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `max_connections_per_server` int DEFAULT NULL,
  `request_timeout_seconds` int DEFAULT NULL,
  `allowed_http_versions` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `dangerous_accept_any_server_certificate` tinyint(1) DEFAULT '0',
  `metadata` json DEFAULT NULL,
  `is_enabled` tinyint(1) DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `created_by` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `updated_by` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `cluster_id` (`cluster_id`),
  KEY `idx_gateway_clusters_is_enabled` (`is_enabled`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='缃戝叧闆嗙兢閰嶇疆琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gateway_routes`
--

DROP TABLE IF EXISTS `gateway_routes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gateway_routes` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL DEFAULT (uuid()),
  `route_id` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `name` varchar(200) COLLATE utf8mb4_general_ci NOT NULL,
  `description` text COLLATE utf8mb4_general_ci,
  `cluster_id` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `match_path` varchar(500) COLLATE utf8mb4_general_ci NOT NULL,
  `match_methods` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `match_hosts` text COLLATE utf8mb4_general_ci,
  `match_headers` json DEFAULT NULL,
  `match_query_parameters` json DEFAULT NULL,
  `transform_path_prefix` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `transform_path_remove_prefix` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `transform_request_headers` json DEFAULT NULL,
  `transform_response_headers` json DEFAULT NULL,
  `authorization_policy` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `rate_limiter_policy` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `cors_policy` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `timeout_seconds` int DEFAULT NULL,
  `sort_order` int NOT NULL DEFAULT '0',
  `is_enabled` tinyint(1) DEFAULT '1',
  `metadata` json DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `created_by` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `updated_by` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `route_id` (`route_id`),
  KEY `idx_gateway_routes_cluster_id` (`cluster_id`),
  KEY `idx_gateway_routes_is_enabled` (`is_enabled`),
  KEY `idx_gateway_routes_order` (`sort_order`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='缃戝叧璺敱閰嶇疆琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `identity_resource_claims`
--

DROP TABLE IF EXISTS `identity_resource_claims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `identity_resource_claims` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鍞竴鏍囪瘑',
  `identity_resource_id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '韬唤璧勬簮ID',
  `type` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Claim绫诲瀷',
  `created_at` datetime NOT NULL COMMENT '鍒涘缓鏃堕棿',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_identity_resource_claims` (`identity_resource_id`,`type`),
  KEY `idx_identity_resource_claims_resource_id` (`identity_resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='韬唤璧勬簮澹版槑琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `identity_resources`
--

DROP TABLE IF EXISTS `identity_resources`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `identity_resources` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '韬唤璧勬簮鍞竴鏍囪瘑',
  `name` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT '韬唤璧勬簮鍚嶇О',
  `display_name` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鏄剧ず鍚嶇О',
  `description` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎻忚堪淇℃伅',
  `enabled` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鏄惁鍚敤',
  `required` tinyint(1) NOT NULL DEFAULT '0' COMMENT '鐢ㄦ埛鏄惁蹇呴』鍚屾剰',
  `emphasize` tinyint(1) NOT NULL DEFAULT '0' COMMENT '鏄惁鍦ㄥ悓鎰忕晫闈腑寮鸿皟',
  `show_in_discovery_document` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鏄惁鍦ㄥ彂鐜版枃妗ｄ腑鏄剧ず',
  `properties` text COLLATE utf8mb4_general_ci COMMENT '鎵╁睍灞炴€?JSON瀵硅薄)',
  `timestamp` bigint NOT NULL COMMENT '鏃堕棿鎴?,
  `created_at` datetime NOT NULL COMMENT '鍒涘缓鏃堕棿',
  `updated_at` datetime DEFAULT NULL COMMENT '鏇存柊鏃堕棿',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_identity_resources_name` (`name`),
  KEY `idx_identity_resources_enabled` (`enabled`),
  KEY `idx_identity_resources_created_at` (`created_at`),
  KEY `idx_timestamp` (`timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='韬唤璧勬簮琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ip_access_rules`
--

DROP TABLE IF EXISTS `ip_access_rules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ip_access_rules` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL DEFAULT (uuid()),
  `ip_address` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `rule_type` varchar(20) COLLATE utf8mb4_general_ci NOT NULL DEFAULT 'blacklist',
  `description` text COLLATE utf8mb4_general_ci,
  `is_enabled` tinyint(1) DEFAULT '1',
  `expires_at` datetime DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `created_by` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_ip_address` (`ip_address`),
  KEY `idx_rule_type` (`rule_type`),
  KEY `idx_is_enabled` (`is_enabled`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='IP璁块棶瑙勫垯琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `openiddict_applications`
--

DROP TABLE IF EXISTS `openiddict_applications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `openiddict_applications` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '涓婚敭 (GUID)',
  `client_id` varchar(100) COLLATE utf8mb4_general_ci NOT NULL COMMENT '瀹㈡埛绔疘D',
  `client_secret` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '瀹㈡埛绔瘑閽ワ紙鍝堝笇鍚庯級',
  `display_name` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鏄剧ず鍚嶇О',
  `type` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '瀹㈡埛绔被鍨嬶紙confidential, public锛?,
  `consent_type` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鍚屾剰绫诲瀷锛坋xplicit, implicit, systematic锛?,
  `permissions` text COLLATE utf8mb4_general_ci COMMENT '鏉冮檺鍒楄〃锛圝SON鏍煎紡锛?,
  `redirect_uris` text COLLATE utf8mb4_general_ci COMMENT '閲嶅畾鍚慤RI鍒楄〃锛圝SON鏍煎紡锛?,
  `post_logout_redirect_uris` text COLLATE utf8mb4_general_ci COMMENT '鐧诲嚭鍚庨噸瀹氬悜URI鍒楄〃锛圝SON鏍煎紡锛?,
  `requirements` text COLLATE utf8mb4_general_ci COMMENT '瑕佹眰鍒楄〃锛圝SON鏍煎紡锛?,
  `properties` text COLLATE utf8mb4_general_ci COMMENT '鎵╁睍灞炴€э紙JSON鏍煎紡锛?,
  `timestamp` bigint NOT NULL COMMENT 'Unix鏃堕棿鎴?姣锛圲TC锛岀敤浜庣储寮曞拰鍒嗛〉锛?,
  `created_at` datetime NOT NULL COMMENT '鍒涘缓鏃堕棿锛圲TC锛?,
  `updated_at` datetime DEFAULT NULL COMMENT '鏇存柊鏃堕棿锛圲TC锛?,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_client_id` (`client_id`),
  KEY `idx_type` (`type`),
  KEY `idx_timestamp` (`timestamp`),
  KEY `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict搴旂敤绋嬪簭琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `openiddict_authorizations`
--

DROP TABLE IF EXISTS `openiddict_authorizations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `openiddict_authorizations` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '涓婚敭 (GUID)',
  `application_id` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鍏宠仈鐨勫簲鐢ㄧ▼搴廔D',
  `subject` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鐢ㄦ埛鏍囪瘑',
  `type` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎺堟潈绫诲瀷',
  `status` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎺堟潈鐘舵€侊紙valid, revoked锛?,
  `scopes` text COLLATE utf8mb4_general_ci COMMENT '鎺堟潈鐨勪綔鐢ㄥ煙鍒楄〃锛圝SON鏍煎紡锛?,
  `properties` text COLLATE utf8mb4_general_ci COMMENT '鎵╁睍灞炴€э紙JSON鏍煎紡锛?,
  `timestamp` bigint NOT NULL COMMENT 'Unix鏃堕棿鎴?姣锛圲TC锛岀敤浜庣储寮曞拰鍒嗛〉锛?,
  `created_at` datetime NOT NULL COMMENT '鍒涘缓鏃堕棿锛圲TC锛?,
  PRIMARY KEY (`id`),
  KEY `idx_application_id` (`application_id`),
  KEY `idx_subject` (`subject`),
  KEY `idx_status` (`status`),
  KEY `idx_timestamp` (`timestamp`),
  KEY `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict鎺堟潈琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `openiddict_scopes`
--

DROP TABLE IF EXISTS `openiddict_scopes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `openiddict_scopes` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '涓婚敭 (GUID)',
  `name` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT '浣滅敤鍩熷悕绉?,
  `display_name` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鏄剧ず鍚嶇О',
  `description` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎻忚堪',
  `resources` text COLLATE utf8mb4_general_ci COMMENT '璧勬簮鍒楄〃锛圝SON鏍煎紡锛?,
  `properties` text COLLATE utf8mb4_general_ci COMMENT '鎵╁睍灞炴€э紙JSON鏍煎紡锛?,
  `timestamp` bigint NOT NULL COMMENT 'Unix鏃堕棿鎴?姣锛圲TC锛岀敤浜庣储寮曞拰鍒嗛〉锛?,
  `created_at` datetime NOT NULL COMMENT '鍒涘缓鏃堕棿锛圲TC锛?,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_name` (`name`),
  KEY `idx_timestamp` (`timestamp`),
  KEY `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict浣滅敤鍩熻〃';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `openiddict_tokens`
--

DROP TABLE IF EXISTS `openiddict_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `openiddict_tokens` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '涓婚敭 (GUID)',
  `application_id` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鍏宠仈鐨勫簲鐢ㄧ▼搴廔D',
  `authorization_id` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鍏宠仈鐨勬巿鏉僆D',
  `subject` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鐢ㄦ埛鏍囪瘑',
  `type` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '浠ょ墝绫诲瀷锛坅ccess_token, refresh_token, id_token锛?,
  `status` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '浠ょ墝鐘舵€侊紙valid, revoked, redeemed锛?,
  `payload` text COLLATE utf8mb4_general_ci COMMENT '浠ょ墝璐熻浇锛圝WT锛?,
  `reference_id` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '寮曠敤ID锛堢敤浜庝护鐗屽唴鐪侊級',
  `expires_at` datetime DEFAULT NULL COMMENT '杩囨湡鏃堕棿锛圲TC锛?,
  `timestamp` bigint NOT NULL COMMENT 'Unix鏃堕棿鎴?姣锛圲TC锛岀敤浜庣储寮曞拰鍒嗛〉锛?,
  `created_at` datetime NOT NULL COMMENT '鍒涘缓鏃堕棿锛圲TC锛?,
  PRIMARY KEY (`id`),
  KEY `idx_application_id` (`application_id`),
  KEY `idx_authorization_id` (`authorization_id`),
  KEY `idx_subject` (`subject`),
  KEY `idx_reference_id` (`reference_id`),
  KEY `idx_status` (`status`),
  KEY `idx_expires_at` (`expires_at`),
  KEY `idx_timestamp` (`timestamp`),
  KEY `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict浠ょ墝琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `permissions`
--

DROP TABLE IF EXISTS `permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `permissions` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `code` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `name` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `description` text COLLATE utf8mb4_general_ci,
  `resource` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `action` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `category` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `is_system` tinyint(1) DEFAULT '0',
  `is_active` tinyint(1) DEFAULT '1',
  `display_order` int DEFAULT '0',
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `updated_by` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `timestamp` bigint NOT NULL DEFAULT ((unix_timestamp() * 1000)),
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`),
  KEY `idx_permissions_code` (`code`),
  KEY `idx_permissions_resource` (`resource`),
  KEY `idx_permissions_category` (`category`),
  KEY `idx_permissions_is_active` (`is_active`),
  KEY `idx_timestamp` (`timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='鏉冮檺琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `rate_limit_policies`
--

DROP TABLE IF EXISTS `rate_limit_policies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rate_limit_policies` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL DEFAULT (uuid()),
  `name` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `display_name` varchar(200) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `policy_type` varchar(50) COLLATE utf8mb4_general_ci NOT NULL DEFAULT 'fixed-window',
  `permit_limit` int NOT NULL DEFAULT '100',
  `window_seconds` int NOT NULL DEFAULT '60',
  `segments_per_window` int DEFAULT '6',
  `queue_limit` int DEFAULT '0',
  `tokens_per_period` int DEFAULT '10',
  `replenishment_period_seconds` int DEFAULT '1',
  `is_enabled` tinyint(1) DEFAULT '1',
  `description` text COLLATE utf8mb4_general_ci,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`),
  KEY `idx_name` (`name`),
  KEY `idx_is_enabled` (`is_enabled`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='闄愭祦绛栫暐琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `request_logs`
--

DROP TABLE IF EXISTS `request_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `request_logs` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `request_id` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `method` varchar(10) COLLATE utf8mb4_general_ci NOT NULL,
  `path` varchar(2048) COLLATE utf8mb4_general_ci NOT NULL,
  `query_string` varchar(2048) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `status_code` int NOT NULL,
  `response_time_ms` bigint NOT NULL,
  `client_ip` varchar(45) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `user_agent` varchar(1024) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `user_id` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `user_name` varchar(256) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `request_time` datetime NOT NULL,
  `request_body_size` bigint DEFAULT NULL,
  `response_body_size` bigint DEFAULT NULL,
  `exception` text COLLATE utf8mb4_general_ci,
  `additional_info` json DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_request_logs_request_time` (`request_time`),
  KEY `idx_request_logs_status_code` (`status_code`),
  KEY `idx_request_logs_user_id` (`user_id`),
  KEY `idx_request_logs_client_ip` (`client_ip`),
  KEY `idx_request_logs_path` (`path`(255)),
  KEY `idx_request_logs_method` (`method`),
  KEY `idx_request_logs_time_status` (`request_time`,`status_code`),
  KEY `idx_request_logs_time_path` (`request_time`,`path`(255))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='API璇锋眰鏃ュ織琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `role_permissions`
--

DROP TABLE IF EXISTS `role_permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role_permissions` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `role_id` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `permission_id` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_role_permission` (`role_id`,`permission_id`),
  KEY `idx_role_permissions_role_id` (`role_id`),
  KEY `idx_role_permissions_permission_id` (`permission_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='瑙掕壊鏉冮檺鍏宠仈琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '瑙掕壊ID (GUID)',
  `name` varchar(50) COLLATE utf8mb4_general_ci NOT NULL COMMENT '瑙掕壊鍚嶇О锛堝敮涓€鏍囪瘑锛屽admin銆乽ser銆乵anager锛?,
  `display_name` varchar(100) COLLATE utf8mb4_general_ci NOT NULL COMMENT '瑙掕壊鏄剧ず鍚嶇О',
  `description` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '瑙掕壊鎻忚堪',
  `is_system` tinyint(1) NOT NULL DEFAULT '0' COMMENT '鏄惁涓虹郴缁熻鑹诧紙绯荤粺瑙掕壊涓嶅彲鍒犻櫎锛?,
  `is_active` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鏄惁鍚敤',
  `permissions` json DEFAULT NULL COMMENT '瑙掕壊鏉冮檺鍒楄〃锛圝SON鏁扮粍锛?,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '鍒涘缓鏃堕棿',
  `created_by` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鍒涘缓鑰匢D',
  `updated_at` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '鏇存柊鏃堕棿',
  `updated_by` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鏇存柊鑰匢D',
  `deleted_at` datetime DEFAULT NULL COMMENT '杞垹闄ゆ椂闂?,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_roles_name` (`name`),
  KEY `idx_roles_is_active` (`is_active`),
  KEY `idx_roles_deleted_at` (`deleted_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='瑙掕壊琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `system_configs`
--

DROP TABLE IF EXISTS `system_configs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `system_configs` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '涓婚敭 (GUID)',
  `name` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT '绫诲瀷鍚嶇О锛圕lient锛孖dentityResource锛孉piResource锛孉piScope绛夛級',
  `key` varchar(200) COLLATE utf8mb4_general_ci NOT NULL COMMENT '閿?,
  `value` text COLLATE utf8mb4_general_ci COMMENT '鍊?,
  `description` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎻忚堪璇存槑',
  `non_editable` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鐢ㄦ埛鏄惁鍙紪杈戯紙0=鍙紪杈戯紝1=涓嶅彲缂栬緫锛?,
  `timestamp` bigint NOT NULL COMMENT 'Unix鏃堕棿鎴?姣锛圲TC锛岀敤浜庣储寮曞拰鍒嗛〉锛?,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '鍒涘缓鏃堕棿锛圲TC锛?,
  `updated_at` datetime DEFAULT NULL COMMENT '鏇存柊鏃堕棿锛圲TC锛?,
  PRIMARY KEY (`id`),
  KEY `idx_name` (`name`),
  KEY `idx_key` (`key`),
  KEY `idx_name_key` (`name`,`key`),
  KEY `idx_timestamp` (`timestamp`),
  KEY `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='绯荤粺鍏冩暟鎹〃';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `system_logs`
--

DROP TABLE IF EXISTS `system_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `system_logs` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鏃ュ織鍞竴鏍囪瘑(GUID)',
  `level` varchar(20) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鏃ュ織绾у埆',
  `message` text COLLATE utf8mb4_general_ci NOT NULL COMMENT '鏃ュ織娑堟伅',
  `exception` text COLLATE utf8mb4_general_ci COMMENT '寮傚父淇℃伅',
  `stack_trace` text COLLATE utf8mb4_general_ci COMMENT '寮傚父鍫嗘爤璺熻釜',
  `source` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '寮傚父鏉ユ簮',
  `user_id` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎿嶄綔鐢ㄦ埛ID',
  `username` varchar(256) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎿嶄綔鐢ㄦ埛鍚?,
  `ip_address` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'IP鍦板潃',
  `user_agent` text COLLATE utf8mb4_general_ci COMMENT '鐢ㄦ埛浠ｇ悊',
  `request_path` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '璇锋眰璺緞',
  `request_method` varchar(10) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '璇锋眰鏂规硶',
  `status_code` int DEFAULT NULL COMMENT 'HTTP鐘舵€佺爜',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '鍒涘缓鏃堕棿',
  `timestamp` bigint NOT NULL COMMENT '鏃堕棿鎴?,
  PRIMARY KEY (`id`),
  KEY `idx_level` (`level`),
  KEY `idx_created_at` (`created_at` DESC),
  KEY `idx_timestamp` (`timestamp` DESC),
  KEY `idx_user_id` (`user_id`),
  KEY `idx_username` (`username`),
  KEY `idx_level_created` (`level`,`created_at` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='绯荤粺鏃ュ織琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `user_roles`
--

DROP TABLE IF EXISTS `user_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_roles` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鍏宠仈ID (GUID)',
  `user_id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鐢ㄦ埛ID',
  `role_id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '瑙掕壊ID',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '鍒嗛厤鏃堕棿',
  `created_by` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鍒嗛厤鑰匢D',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_user_roles` (`user_id`,`role_id`),
  KEY `idx_user_roles_user_id` (`user_id`),
  KEY `idx_user_roles_role_id` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='鐢ㄦ埛瑙掕壊鍏宠仈琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` char(36) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鐢ㄦ埛鍞竴鏍囪瘑(GUID)',
  `username` varchar(50) COLLATE utf8mb4_general_ci NOT NULL COMMENT '鐢ㄦ埛鍚嶏紙鐧诲綍鍚嶏級',
  `password_hash` varchar(255) COLLATE utf8mb4_general_ci NOT NULL COMMENT '瀵嗙爜鍝堝笇',
  `email` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '閭',
  `phone_number` varchar(20) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鎵嬫満鍙?,
  `display_name` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鏄剧ず鍚嶇О',
  `avatar` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '澶村儚URL',
  `role` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '瑙掕壊锛坅dmin, user, manager绛夛級',
  `is_active` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鏄惁婵€娲?,
  `email_confirmed` tinyint(1) NOT NULL DEFAULT '0' COMMENT '閭鏄惁宸查獙璇?,
  `phone_number_confirmed` tinyint(1) NOT NULL DEFAULT '0' COMMENT '鎵嬫満鍙锋槸鍚﹀凡楠岃瘉',
  `last_login_at` datetime DEFAULT NULL COMMENT '鏈€鍚庣櫥褰曟椂闂?,
  `failed_login_count` int NOT NULL DEFAULT '0' COMMENT '杩炵画鐧诲綍澶辫触娆℃暟',
  `lockout_end` datetime DEFAULT NULL COMMENT '閿佸畾缁撴潫鏃堕棿',
  `lockout_enabled` tinyint(1) NOT NULL DEFAULT '1' COMMENT '鏄惁鍚敤閿佸畾鍔熻兘',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '鍒涘缓鏃堕棿',
  `updated_at` datetime DEFAULT NULL COMMENT '鏇存柊鏃堕棿',
  `created_by` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鍒涘缓鑰匢D',
  `updated_by` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '鏇存柊鑰匢D',
  `remark` varchar(500) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '澶囨敞',
  `timestamp` bigint NOT NULL COMMENT '鏃堕棿鎴筹紙鐢ㄤ簬鍒嗛〉鏌ヨ锛?,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_username` (`username`),
  UNIQUE KEY `uk_email` (`email`),
  KEY `idx_role` (`role`),
  KEY `idx_is_active` (`is_active`),
  KEY `idx_created_at` (`created_at` DESC),
  KEY `idx_timestamp` (`timestamp`),
  KEY `idx_users_lockout` (`username`,`lockout_end`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='鐢ㄦ埛琛?;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping routines for database 'dawning_identity'
--
/*!50003 DROP PROCEDURE IF EXISTS `sp_prune_expired_tokens` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`aluneth`@`%` PROCEDURE `sp_prune_expired_tokens`()
BEGIN

    DELETE FROM `openiddict_tokens`

    WHERE `expires_at` IS NOT NULL 

      AND `expires_at` < UTC_TIMESTAMP()  -- 浣跨敤 UTC_TIMESTAMP()

      AND `status` != 'valid';

      

    SELECT ROW_COUNT() AS deleted_count;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-12-22 16:52:54
