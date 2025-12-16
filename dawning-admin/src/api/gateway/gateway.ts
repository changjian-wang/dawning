import axios from '@/api/interceptor';
import type { IPagedData } from '../paged-data';

// ==================== 路由相关接口 ====================

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
  order: number;
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

// ==================== 集群相关接口 ====================

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

// ==================== 路由 API ====================

/**
 * 分页获取路由列表
 */
export async function getRoutePagedList(
  params: GatewayRouteQueryParams
): Promise<IPagedData<GatewayRoute>> {
  const response = await axios.get('/api/gateway/route/paged', { params });
  return response.data as IPagedData<GatewayRoute>;
}

/**
 * 获取所有启用的路由
 */
export async function getEnabledRoutes(): Promise<GatewayRoute[]> {
  const response = await axios.get('/api/gateway/route/enabled');
  return response.data as GatewayRoute[];
}

/**
 * 根据ID获取路由
 */
export async function getRoute(id: string): Promise<GatewayRoute> {
  const response = await axios.get(`/api/gateway/route/${id}`);
  return response.data as GatewayRoute;
}

/**
 * 创建路由
 */
export async function createRoute(
  data: CreateGatewayRouteDto
): Promise<GatewayRoute> {
  const response = await axios.post('/api/gateway/route', data);
  return response.data as GatewayRoute;
}

/**
 * 更新路由
 */
export async function updateRoute(
  id: string,
  data: UpdateGatewayRouteDto
): Promise<GatewayRoute> {
  const response = await axios.put(`/api/gateway/route/${id}`, data);
  return response.data as GatewayRoute;
}

/**
 * 删除路由
 */
export async function deleteRoute(id: string): Promise<void> {
  await axios.delete(`/api/gateway/route/${id}`);
}

/**
 * 切换路由启用状态
 */
export async function toggleRouteEnabled(
  id: string,
  isEnabled: boolean
): Promise<void> {
  await axios.patch(`/api/gateway/route/${id}/toggle`, { isEnabled });
}

// ==================== 集群 API ====================

/**
 * 分页获取集群列表
 */
export async function getClusterPagedList(
  params: GatewayClusterQueryParams
): Promise<IPagedData<GatewayCluster>> {
  const response = await axios.get('/api/gateway/cluster/paged', { params });
  return response.data as IPagedData<GatewayCluster>;
}

/**
 * 获取所有启用的集群
 */
export async function getEnabledClusters(): Promise<GatewayCluster[]> {
  const response = await axios.get('/api/gateway/cluster/enabled');
  return response.data as GatewayCluster[];
}

/**
 * 获取集群选项（用于下拉选择）
 */
export async function getClusterOptions(): Promise<ClusterOption[]> {
  const response = await axios.get('/api/gateway/cluster/options');
  return response.data as ClusterOption[];
}

/**
 * 根据ID获取集群
 */
export async function getCluster(id: string): Promise<GatewayCluster> {
  const response = await axios.get(`/api/gateway/cluster/${id}`);
  return response.data as GatewayCluster;
}

/**
 * 创建集群
 */
export async function createCluster(
  data: CreateGatewayClusterDto
): Promise<GatewayCluster> {
  const response = await axios.post('/api/gateway/cluster', data);
  return response.data as GatewayCluster;
}

/**
 * 更新集群
 */
export async function updateCluster(
  id: string,
  data: UpdateGatewayClusterDto
): Promise<GatewayCluster> {
  const response = await axios.put(`/api/gateway/cluster/${id}`, data);
  return response.data as GatewayCluster;
}

/**
 * 删除集群
 */
export async function deleteCluster(id: string): Promise<void> {
  await axios.delete(`/api/gateway/cluster/${id}`);
}

/**
 * 切换集群启用状态
 */
export async function toggleClusterEnabled(
  id: string,
  isEnabled: boolean
): Promise<void> {
  await axios.patch(`/api/gateway/cluster/${id}/toggle`, { isEnabled });
}

// 负载均衡策略选项
export const loadBalancingPolicies = [
  { value: 'RoundRobin', label: '轮询 (Round Robin)' },
  { value: 'Random', label: '随机 (Random)' },
  { value: 'LeastRequests', label: '最少请求 (Least Requests)' },
  { value: 'PowerOfTwoChoices', label: '二选一 (Power of Two Choices)' },
  { value: 'First', label: '第一个 (First)' },
];

// 授权策略选项
export const authorizationPolicies = [
  { value: '', label: '无' },
  { value: 'RequireAuthenticatedUser', label: '需要认证用户' },
  { value: 'RequireAdmin', label: '需要管理员' },
];

// 限流策略选项
export const rateLimiterPolicies = [
  { value: '', label: '无' },
  { value: 'FixedWindowPolicy', label: '固定窗口' },
  { value: 'SlidingWindowPolicy', label: '滑动窗口' },
];
