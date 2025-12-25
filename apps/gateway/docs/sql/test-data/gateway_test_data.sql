-- Insert test cluster data
INSERT INTO gateway_clusters (cluster_id, name, description, destinations, load_balancing_policy, health_check_enabled, health_check_path, is_enabled)
VALUES 
('identity-cluster', 'Identity Service', 'Identity authentication service cluster', '{"destination1": {"Address": "http://localhost:5195"}}', 'RoundRobin', 1, '/health', 1)
ON DUPLICATE KEY UPDATE name=VALUES(name), destinations=VALUES(destinations);

-- Insert test route data
INSERT INTO gateway_routes (route_id, name, cluster_id, match_path, transform_path_remove_prefix, `order`, is_enabled)
VALUES 
('identity-route', 'Identity API Route', 'identity-cluster', '/api/identity/{**catch-all}', '/api/identity', 0, 1)
ON DUPLICATE KEY UPDATE name=VALUES(name);

-- Show results
SELECT 'Clusters:' as Info;
SELECT cluster_id, name, is_enabled FROM gateway_clusters;
SELECT 'Routes:' as Info;
SELECT route_id, name, cluster_id, match_path, is_enabled FROM gateway_routes;
