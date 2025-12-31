import axios from 'axios';

// ==================== Rate Limit Policy ====================

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

// Get all rate limit policies
export function getAllPolicies() {
  return axios.get<{ success: boolean; data: RateLimitPolicy[] }>(
    '/api/rate-limit/policies'
  );
}

// Get rate limit policy details
export function getPolicy(id: string) {
  return axios.get<{ success: boolean; data: RateLimitPolicy }>(
    `/api/rate-limit/policies/${id}`
  );
}

// Create rate limit policy
export function createPolicy(data: CreateRateLimitPolicy) {
  return axios.post<{ success: boolean; data: { id: string } }>(
    '/api/rate-limit/policies',
    data
  );
}

// Update rate limit policy
export function updatePolicy(id: string, data: UpdateRateLimitPolicy) {
  return axios.put<{ success: boolean }>(
    `/api/rate-limit/policies/${id}`,
    data
  );
}

// Delete rate limit policy
export function deletePolicy(id: string) {
  return axios.delete<{ success: boolean }>(`/api/rate-limit/policies/${id}`);
}

// ==================== IP Access Rules ====================

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

// Get IP rules list
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

// Get IP rule details
export function getIpRule(id: string) {
  return axios.get<{ success: boolean; data: IpAccessRule }>(
    `/api/rate-limit/ip-rules/${id}`
  );
}

// Get blacklist
export function getBlacklist() {
  return axios.get<{ success: boolean; data: IpAccessRule[] }>(
    '/api/rate-limit/ip-rules/blacklist'
  );
}

// Get whitelist
export function getWhitelist() {
  return axios.get<{ success: boolean; data: IpAccessRule[] }>(
    '/api/rate-limit/ip-rules/whitelist'
  );
}

// Create IP rule
export function createIpRule(data: CreateIpAccessRule) {
  return axios.post<{ success: boolean; data: { id: string } }>(
    '/api/rate-limit/ip-rules',
    data
  );
}

// Update IP rule
export function updateIpRule(id: string, data: UpdateIpAccessRule) {
  return axios.put<{ success: boolean }>(
    `/api/rate-limit/ip-rules/${id}`,
    data
  );
}

// Delete IP rule
export function deleteIpRule(id: string) {
  return axios.delete<{ success: boolean }>(`/api/rate-limit/ip-rules/${id}`);
}

// Check if IP is in blacklist
export function checkBlacklist(ip: string) {
  return axios.get<{
    success: boolean;
    data: { ip: string; isBlacklisted: boolean };
  }>('/api/rate-limit/check-blacklist', { params: { ip } });
}

// Check if IP is in whitelist
export function checkWhitelist(ip: string) {
  return axios.get<{
    success: boolean;
    data: { ip: string; isWhitelisted: boolean };
  }>('/api/rate-limit/check-whitelist', { params: { ip } });
}

// ==================== Constants ====================

export const policyTypes = [
  { value: 'fixed-window', label: 'Fixed Window' },
  { value: 'sliding-window', label: 'Sliding Window' },
  { value: 'token-bucket', label: 'Token Bucket' },
  { value: 'concurrency', label: 'Concurrency Limit' },
];

export const ruleTypes = [
  { value: 'blacklist', label: 'Blacklist' },
  { value: 'whitelist', label: 'Whitelist' },
];
