import axios from '@/api/interceptor';
import type { IPagedData } from '../paged-data';

// ==================== Route Related Interfaces ====================

export interface GatewayRoute {
  id: string;
  routeId: string;
  name: string;
  description?: string;
  clusterId: string;
  matchPath: string;
  matchMethods?: string;
  matchHosts?: string;
  matchHeaders?: string;
  matchQueryParameters?: string;
  transformPathPrefix?: string;
  transformPathRemovePrefix?: string;
  transformRequestHeaders?: string;
  transformResponseHeaders?: string;
  authorizationPolicy?: string;
  rateLimiterPolicy?: string;
  corsPolicy?: string;
  timeoutSeconds?: number;
  order: number;
  isEnabled: boolean;
  metadata?: string;
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface CreateGatewayRouteDto {
  routeId: string;
  name: string;
  description?: string;
  clusterId: string;
  matchPath: string;
  matchMethods?: string;
  matchHosts?: string;
  matchHeaders?: string;
  matchQueryParameters?: string;
  transformPathPrefix?: string;
  transformPathRemovePrefix?: string;
  transformRequestHeaders?: string;
  transformResponseHeaders?: string;
  authorizationPolicy?: string;
  rateLimiterPolicy?: string;
  corsPolicy?: string;
  timeoutSeconds?: number;
  sortOrder: number;
  isEnabled: boolean;
  metadata?: string;
}

export interface UpdateGatewayRouteDto extends CreateGatewayRouteDto {
  id: string;
}

export interface GatewayRouteQueryParams {
  routeId?: string;
  name?: string;
  clusterId?: string;
  matchPath?: string;
  isEnabled?: boolean;
  keyword?: string;
  page?: number;
  pageSize?: number;
}

// ==================== Cluster Related Interfaces ====================

export interface GatewayCluster {
  id: string;
  clusterId: string;
  name: string;
  description?: string;
  loadBalancingPolicy: string;
  destinations: string;
  healthCheckEnabled: boolean;
  healthCheckInterval?: number;
  healthCheckTimeout?: number;
  healthCheckPath?: string;
  passiveHealthPolicy?: string;
  sessionAffinityEnabled: boolean;
  sessionAffinityPolicy?: string;
  sessionAffinityKeyName?: string;
  maxConnectionsPerServer?: number;
  requestTimeoutSeconds?: number;
  allowedHttpVersions?: string;
  dangerousAcceptAnyServerCertificate: boolean;
  metadata?: string;
  isEnabled: boolean;
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface ClusterDestination {
  destinationId: string;
  address: string;
  health?: string;
  metadata?: string;
}

export interface CreateGatewayClusterDto {
  clusterId: string;
  name: string;
  description?: string;
  loadBalancingPolicy: string;
  destinations: string;
  healthCheckEnabled: boolean;
  healthCheckInterval?: number;
  healthCheckTimeout?: number;
  healthCheckPath?: string;
  passiveHealthPolicy?: string;
  sessionAffinityEnabled: boolean;
  sessionAffinityPolicy?: string;
  sessionAffinityKeyName?: string;
  maxConnectionsPerServer?: number;
  requestTimeoutSeconds?: number;
  allowedHttpVersions?: string;
  dangerousAcceptAnyServerCertificate: boolean;
  metadata?: string;
  isEnabled: boolean;
}

export interface UpdateGatewayClusterDto extends CreateGatewayClusterDto {
  id: string;
}

export interface GatewayClusterQueryParams {
  clusterId?: string;
  name?: string;
  loadBalancingPolicy?: string;
  isEnabled?: boolean;
  keyword?: string;
  page?: number;
  pageSize?: number;
}

export interface ClusterOption {
  clusterId: string;
  name: string;
  isEnabled: boolean;
}

// ==================== Route API ====================

/**
 * Get route list (paginated)
 */
export async function getRoutePagedList(
  params: GatewayRouteQueryParams
): Promise<IPagedData<GatewayRoute>> {
  const response = await axios.get('/api/gateway/route/paged', { params });
  return response.data as IPagedData<GatewayRoute>;
}

/**
 * Get all enabled routes
 */
export async function getEnabledRoutes(): Promise<GatewayRoute[]> {
  const response = await axios.get('/api/gateway/route/enabled');
  return response.data as GatewayRoute[];
}

/**
 * Get route by ID
 */
export async function getRoute(id: string): Promise<GatewayRoute> {
  const response = await axios.get(`/api/gateway/route/${id}`);
  return response.data as GatewayRoute;
}

/**
 * Create route
 */
export async function createRoute(
  data: CreateGatewayRouteDto
): Promise<GatewayRoute> {
  const response = await axios.post('/api/gateway/route', data);
  return response.data as GatewayRoute;
}

/**
 * Update route
 */
export async function updateRoute(
  id: string,
  data: UpdateGatewayRouteDto
): Promise<GatewayRoute> {
  const response = await axios.put(`/api/gateway/route/${id}`, data);
  return response.data as GatewayRoute;
}

/**
 * Delete route
 */
export async function deleteRoute(id: string): Promise<void> {
  await axios.delete(`/api/gateway/route/${id}`);
}

/**
 * Toggle route enabled status
 */
export async function toggleRouteEnabled(
  id: string,
  isEnabled: boolean
): Promise<void> {
  await axios.patch(`/api/gateway/route/${id}/toggle`, { isEnabled });
}

// ==================== Cluster API ====================

/**
 * Get cluster list (paginated)
 */
export async function getClusterPagedList(
  params: GatewayClusterQueryParams
): Promise<IPagedData<GatewayCluster>> {
  const response = await axios.get('/api/gateway/cluster/paged', { params });
  return response.data as IPagedData<GatewayCluster>;
}

/**
 * Get all enabled clusters
 */
export async function getEnabledClusters(): Promise<GatewayCluster[]> {
  const response = await axios.get('/api/gateway/cluster/enabled');
  return response.data as GatewayCluster[];
}

/**
 * Get cluster options (for dropdown selection)
 */
export async function getClusterOptions(): Promise<ClusterOption[]> {
  const response = await axios.get('/api/gateway/cluster/options');
  return response.data as ClusterOption[];
}

/**
 * Get cluster by ID
 */
export async function getCluster(id: string): Promise<GatewayCluster> {
  const response = await axios.get(`/api/gateway/cluster/${id}`);
  return response.data as GatewayCluster;
}

/**
 * Create cluster
 */
export async function createCluster(
  data: CreateGatewayClusterDto
): Promise<GatewayCluster> {
  const response = await axios.post('/api/gateway/cluster', data);
  return response.data as GatewayCluster;
}

/**
 * Update cluster
 */
export async function updateCluster(
  id: string,
  data: UpdateGatewayClusterDto
): Promise<GatewayCluster> {
  const response = await axios.put(`/api/gateway/cluster/${id}`, data);
  return response.data as GatewayCluster;
}

/**
 * Delete cluster
 */
export async function deleteCluster(id: string): Promise<void> {
  await axios.delete(`/api/gateway/cluster/${id}`);
}

/**
 * Toggle cluster enabled status
 */
export async function toggleClusterEnabled(
  id: string,
  isEnabled: boolean
): Promise<void> {
  await axios.patch(`/api/gateway/cluster/${id}/toggle`, { isEnabled });
}

// Load balancing policy options
export const loadBalancingPolicies = [
  { value: 'RoundRobin', label: 'Round Robin' },
  { value: 'Random', label: 'Random' },
  { value: 'LeastRequests', label: 'Least Requests' },
  { value: 'PowerOfTwoChoices', label: 'Power of Two Choices' },
  { value: 'First', label: 'First' },
];

// Authorization policy options
export const authorizationPolicies = [
  { value: '', label: 'None' },
  { value: 'RequireAuthenticatedUser', label: 'Require Authenticated User' },
  { value: 'RequireAdmin', label: 'Require Admin' },
];

// Rate limiter policy options
export const rateLimiterPolicies = [
  { value: '', label: 'None' },
  { value: 'FixedWindowPolicy', label: 'Fixed Window' },
  { value: 'SlidingWindowPolicy', label: 'Sliding Window' },
];
