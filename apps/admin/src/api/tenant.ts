import axios from '@/api/interceptor';
import type { HttpResponse } from './interceptor';

// Tenant type definitions
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

// Get current tenant information
export function getCurrentTenant() {
  return axios.get<HttpResponse<CurrentTenant>>('/api/tenant/current');
}

// Get tenant list (paginated)
// Note: Interceptor will unwrap the response, actual return is TenantListResponse
export function getTenantList(params: {
  keyword?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}) {
  return axios.get<TenantListResponse>('/api/tenant', { params });
}

// Get all tenants
export function getAllTenants() {
  return axios.get<HttpResponse<Tenant[]>>('/api/tenant/all');
}

// Get all active tenants
export function getActiveTenants() {
  return axios.get<HttpResponse<Tenant[]>>('/api/tenant/active');
}

// Get tenant by ID
export function getTenantById(id: string) {
  return axios.get<HttpResponse<Tenant>>(`/api/tenant/${id}`);
}

// Create tenant
export function createTenant(data: CreateTenantRequest) {
  return axios.post<HttpResponse<Tenant>>('/api/tenant', data);
}

// Update tenant
export function updateTenant(id: string, data: UpdateTenantRequest) {
  return axios.put<HttpResponse<Tenant>>(`/api/tenant/${id}`, data);
}

// Delete tenant
export function deleteTenant(id: string) {
  return axios.delete<HttpResponse<void>>(`/api/tenant/${id}`);
}

// Set tenant active status
export function setTenantActive(id: string, isActive: boolean) {
  return axios.put<HttpResponse<void>>(`/api/tenant/${id}/active`, { isActive });
}

// Check if tenant code is available
export function checkTenantCode(code: string, excludeId?: string) {
  return axios.get<HttpResponse<{ available: boolean }>>('/api/tenant/check-code', {
    params: { code, excludeId },
  });
}

// Check if domain is available
export function checkTenantDomain(domain: string, excludeId?: string) {
  return axios.get<HttpResponse<{ available: boolean }>>('/api/tenant/check-domain', {
    params: { domain, excludeId },
  });
}
