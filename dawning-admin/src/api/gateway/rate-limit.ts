import axios from 'axios';

// ==================== 限流策略 ====================

export interface RateLimitPolicy {
  id: string;
  name: string;
  displayName?: string;
  policyType: string;
  permitLimit: number;
  windowSeconds: number;
  segmentsPerWindow: number;
  queueLimit: number;
  tokensPerPeriod: number;
  replenishmentPeriodSeconds: number;
  isEnabled: boolean;
  description?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateRateLimitPolicy {
  name: string;
  displayName?: string;
  policyType: string;
  permitLimit: number;
  windowSeconds: number;
  segmentsPerWindow?: number;
  queueLimit?: number;
  tokensPerPeriod?: number;
  replenishmentPeriodSeconds?: number;
  isEnabled: boolean;
  description?: string;
}

export interface UpdateRateLimitPolicy extends CreateRateLimitPolicy {
  id: string;
}

// 获取所有限流策略
export function getAllPolicies() {
  return axios.get<{ success: boolean; data: RateLimitPolicy[] }>(
    '/api/rate-limit/policies'
  );
}

// 获取限流策略详情
export function getPolicy(id: string) {
  return axios.get<{ success: boolean; data: RateLimitPolicy }>(
    `/api/rate-limit/policies/${id}`
  );
}

// 创建限流策略
export function createPolicy(data: CreateRateLimitPolicy) {
  return axios.post<{ success: boolean; data: { id: string } }>(
    '/api/rate-limit/policies',
    data
  );
}

// 更新限流策略
export function updatePolicy(id: string, data: UpdateRateLimitPolicy) {
  return axios.put<{ success: boolean }>(`/api/rate-limit/policies/${id}`, data);
}

// 删除限流策略
export function deletePolicy(id: string) {
  return axios.delete<{ success: boolean }>(`/api/rate-limit/policies/${id}`);
}

// ==================== IP 访问规则 ====================

export interface IpAccessRule {
  id: string;
  ipAddress: string;
  ruleType: 'whitelist' | 'blacklist';
  description?: string;
  isEnabled: boolean;
  expiresAt?: string;
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
}

export interface CreateIpAccessRule {
  ipAddress: string;
  ruleType: 'whitelist' | 'blacklist';
  description?: string;
  isEnabled: boolean;
  expiresAt?: string;
}

export interface UpdateIpAccessRule extends CreateIpAccessRule {
  id: string;
}

export interface IpRulesResponse {
  items: IpAccessRule[];
  total: number;
  page: number;
  pageSize: number;
}

// 获取 IP 规则列表
export function getIpRules(params: {
  ruleType?: string;
  isEnabled?: boolean;
  page?: number;
  pageSize?: number;
}) {
  return axios.get<{ success: boolean; data: IpRulesResponse }>(
    '/api/rate-limit/ip-rules',
    { params }
  );
}

// 获取 IP 规则详情
export function getIpRule(id: string) {
  return axios.get<{ success: boolean; data: IpAccessRule }>(
    `/api/rate-limit/ip-rules/${id}`
  );
}

// 获取黑名单
export function getBlacklist() {
  return axios.get<{ success: boolean; data: IpAccessRule[] }>(
    '/api/rate-limit/ip-rules/blacklist'
  );
}

// 获取白名单
export function getWhitelist() {
  return axios.get<{ success: boolean; data: IpAccessRule[] }>(
    '/api/rate-limit/ip-rules/whitelist'
  );
}

// 创建 IP 规则
export function createIpRule(data: CreateIpAccessRule) {
  return axios.post<{ success: boolean; data: { id: string } }>(
    '/api/rate-limit/ip-rules',
    data
  );
}

// 更新 IP 规则
export function updateIpRule(id: string, data: UpdateIpAccessRule) {
  return axios.put<{ success: boolean }>(`/api/rate-limit/ip-rules/${id}`, data);
}

// 删除 IP 规则
export function deleteIpRule(id: string) {
  return axios.delete<{ success: boolean }>(`/api/rate-limit/ip-rules/${id}`);
}

// 检查 IP 是否在黑名单中
export function checkBlacklist(ip: string) {
  return axios.get<{ success: boolean; data: { ip: string; isBlacklisted: boolean } }>(
    '/api/rate-limit/check-blacklist',
    { params: { ip } }
  );
}

// 检查 IP 是否在白名单中
export function checkWhitelist(ip: string) {
  return axios.get<{ success: boolean; data: { ip: string; isWhitelisted: boolean } }>(
    '/api/rate-limit/check-whitelist',
    { params: { ip } }
  );
}

// ==================== 常量 ====================

export const policyTypes = [
  { value: 'fixed-window', label: '固定窗口' },
  { value: 'sliding-window', label: '滑动窗口' },
  { value: 'token-bucket', label: '令牌桶' },
  { value: 'concurrency', label: '并发限制' },
];

export const ruleTypes = [
  { value: 'blacklist', label: '黑名单' },
  { value: 'whitelist', label: '白名单' },
];
