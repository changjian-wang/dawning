import axios from '@/api/interceptor';
import type { HttpResponse } from './interceptor';

// 租户类型定义
export interface Tenant {
  id: string;
  code: string;
  name: string;
  description?: string;
  domain?: string;
  email?: string;
  phone?: string;
  logoUrl?: string;
  settings?: string;
  connectionString?: string;
  isActive: boolean;
  plan: string;
  subscriptionExpiresAt?: string;
  maxUsers?: number;
  maxStorageMB?: number;
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface TenantListResponse {
  list: Tenant[];
  pagination: {
    current: number;
    pageSize: number;
    total: number;
  };
}

export interface CreateTenantRequest {
  code: string;
  name: string;
  description?: string;
  domain?: string;
  email?: string;
  phone?: string;
  logoUrl?: string;
  plan?: string;
  maxUsers?: number;
  maxStorageMB?: number;
  subscriptionExpiresAt?: string;
  isActive?: boolean;
}

export interface UpdateTenantRequest {
  code?: string;
  name?: string;
  description?: string;
  domain?: string;
  email?: string;
  phone?: string;
  logoUrl?: string;
  plan?: string;
  maxUsers?: number;
  maxStorageMB?: number;
  subscriptionExpiresAt?: string;
  settings?: string;
  isActive?: boolean;
}

export interface CurrentTenant {
  tenantId?: string;
  tenantName?: string;
  isHost: boolean;
}

// 获取当前租户信息
export function getCurrentTenant() {
  return axios.get<HttpResponse<CurrentTenant>>('/api/tenant/current');
}

// 分页获取租户列表
// 注意：拦截器会解包响应，实际返回的是 TenantListResponse
export function getTenantList(params: {
  keyword?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}) {
  return axios.get<TenantListResponse>('/api/tenant', { params });
}

// 获取所有租户
export function getAllTenants() {
  return axios.get<HttpResponse<Tenant[]>>('/api/tenant/all');
}

// 获取所有启用的租户
export function getActiveTenants() {
  return axios.get<HttpResponse<Tenant[]>>('/api/tenant/active');
}

// 根据ID获取租户
export function getTenantById(id: string) {
  return axios.get<HttpResponse<Tenant>>(`/api/tenant/${id}`);
}

// 创建租户
export function createTenant(data: CreateTenantRequest) {
  return axios.post<HttpResponse<Tenant>>('/api/tenant', data);
}

// 更新租户
export function updateTenant(id: string, data: UpdateTenantRequest) {
  return axios.put<HttpResponse<Tenant>>(`/api/tenant/${id}`, data);
}

// 删除租户
export function deleteTenant(id: string) {
  return axios.delete<HttpResponse<void>>(`/api/tenant/${id}`);
}

// 设置租户启用状态
export function setTenantActive(id: string, isActive: boolean) {
  return axios.put<HttpResponse<void>>(`/api/tenant/${id}/active`, { isActive });
}

// 检查租户代码是否可用
export function checkTenantCode(code: string, excludeId?: string) {
  return axios.get<HttpResponse<{ available: boolean }>>('/api/tenant/check-code', {
    params: { code, excludeId },
  });
}

// 检查域名是否可用
export function checkTenantDomain(domain: string, excludeId?: string) {
  return axios.get<HttpResponse<{ available: boolean }>>('/api/tenant/check-domain', {
    params: { domain, excludeId },
  });
}
