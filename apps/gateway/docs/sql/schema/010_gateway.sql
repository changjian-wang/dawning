-- ============================================
-- Gateway Route and Cluster Management Tables (MySQL 8.0+)
-- ============================================

-- Gateway clusters table (must be created first since routes table references clusters)
CREATE TABLE IF NOT EXISTS gateway_clusters (
    id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    cluster_id VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    load_balancing_policy VARCHAR(50) DEFAULT 'RoundRobin',
    destinations JSON NOT NULL,
    health_check_enabled TINYINT(1) DEFAULT 0,
    health_check_interval INT,
    health_check_timeout INT,
    health_check_path VARCHAR(200),
    passive_health_policy VARCHAR(50),
    session_affinity_enabled TINYINT(1) DEFAULT 0,
    session_affinity_policy VARCHAR(50),
    session_affinity_key_name VARCHAR(100),
    max_connections_per_server INT,
    request_timeout_seconds INT,
    allowed_http_versions VARCHAR(50),
    dangerous_accept_any_server_certificate TINYINT(1) DEFAULT 0,
    metadata JSON,
    is_enabled TINYINT(1) DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP,
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Gateway cluster configuration table';

-- Gateway routes table
CREATE TABLE IF NOT EXISTS gateway_routes (
    id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    route_id VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    cluster_id VARCHAR(100) NOT NULL,
    match_path VARCHAR(500) NOT NULL,
    match_methods VARCHAR(200),
    match_hosts TEXT,
    match_headers JSON,
    match_query_parameters JSON,
    transform_path_prefix VARCHAR(200),
    transform_path_remove_prefix VARCHAR(200),
    transform_request_headers JSON,
    transform_response_headers JSON,
    authorization_policy VARCHAR(100),
    rate_limiter_policy VARCHAR(100),
    cors_policy VARCHAR(100),
    timeout_seconds INT,
    sort_order INT DEFAULT 0,
    is_enabled TINYINT(1) DEFAULT 1,
    metadata JSON,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP,
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Gateway route configuration table';

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_gateway_routes_cluster_id ON gateway_routes(cluster_id);
CREATE INDEX IF NOT EXISTS idx_gateway_routes_is_enabled ON gateway_routes(is_enabled);
CREATE INDEX IF NOT EXISTS idx_gateway_routes_sort_order ON gateway_routes(sort_order);
CREATE INDEX IF NOT EXISTS idx_gateway_clusters_is_enabled ON gateway_clusters(is_enabled);

-- Insert default gateway cluster (Identity Service)
INSERT INTO gateway_clusters (cluster_id, name, description, destinations, load_balancing_policy, health_check_enabled, health_check_path, is_enabled)
VALUES 
('identity-cluster', 'Identity Service', 'Identity authentication service cluster', '{"destination1": {"Address": "http://localhost:5202"}}', 'RoundRobin', 1, '/health', 1)
ON DUPLICATE KEY UPDATE name=VALUES(name), destinations=VALUES(destinations);

-- Insert default gateway route
INSERT INTO gateway_routes (route_id, name, cluster_id, match_path, transform_path_remove_prefix, sort_order, is_enabled)
VALUES 
('identity-route', 'Identity API Route', 'identity-cluster', '/api/identity/{**catch-all}', '/api/identity', 0, 1)
ON DUPLICATE KEY UPDATE name=VALUES(name);
