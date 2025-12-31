import axios from '@/api/interceptor';
import type { IPagedData } from '../paged-data';

/**
 * Permission model
 */
export interface PermissionModel {
  id: string;
  code: string;
  name: string;
  description?: string;
  resource: string;
  action: string;
  category: string;
  isSystem: boolean;
  isActive: boolean;
  displayOrder: number;
  createdAt?: string;
  updatedAt?: string;
}

/**
 * Create permission DTO
 */
export interface CreatePermissionDto {
  code: string;
  name: string;
  description?: string;
  resource: string;
  action: string;
  category: string;
  isActive: boolean;
  displayOrder: number;
}

/**
 * Update permission DTO
 */
export interface UpdatePermissionDto {
  name?: string;
  description?: string;
  isActive?: boolean;
  displayOrder?: number;
}

/**
 * Permission query parameters
 */
export interface PermissionQueryParams {
  code?: string;
  name?: string;
  resource?: string;
  action?: string;
  category?: string;
  isActive?: boolean;
  isSystem?: boolean;
  page?: number;
  pageSize?: number;
}

/**
 * Role permission association
 */
export interface RolePermissionDto {
  roleId: string;
  permissionIds: string[];
}

/**
 * Permission group (by resource)
 */
export interface PermissionGroup {
  resource: string;
  permissions: PermissionModel[];
}

/**
 * Get permission list (paginated)
 */
export async function getPermissionList(
  params: PermissionQueryParams
): Promise<IPagedData<PermissionModel>> {
  const response = await axios.get<IPagedData<PermissionModel>>(
    '/api/permission',
    { params }
  );
  // Data wrapped by ApiResponse, interceptor unwraps it to IPagedData format
  return response.data;
}

/**
 * Get all active permissions
 */
export async function getAllActivePermissions(): Promise<PermissionModel[]> {
  const response = await axios.get<PermissionModel[]>('/api/permission/all');
  return response.data;
}

/**
 * Get grouped permissions (grouped by resource)
 */
export async function getGroupedPermissions(): Promise<PermissionGroup[]> {
  const response = await axios.get<PermissionGroup[]>(
    '/api/permission/grouped'
  );
  return response.data;
}

/**
 * Get all resource types
 */
export async function getResourceTypes(): Promise<string[]> {
  const response = await axios.get<string[]>('/api/permission/resources');
  return response.data;
}

/**
 * Get all categories
 */
export async function getCategories(): Promise<string[]> {
  const response = await axios.get<string[]>('/api/permission/categories');
  return response.data;
}

/**
 * Get permission by ID
 */
export async function getPermissionById(id: string): Promise<PermissionModel> {
  const response = await axios.get<PermissionModel>(`/api/permission/${id}`);
  return response.data;
}

/**
 * Get permission by code
 */
export async function getPermissionByCode(
  code: string
): Promise<PermissionModel> {
  const response = await axios.get<PermissionModel>(
    `/api/permission/code/${code}`
  );
  return response.data;
}

/**
 * Get role permissions list
 */
export async function getRolePermissions(
  roleId: string
): Promise<PermissionModel[]> {
  const response = await axios.get<PermissionModel[]>(
    `/api/permission/role/${roleId}`
  );
  return response.data;
}

/**
 * Get user's all permission codes (aggregated from all roles)
 */
export async function getUserPermissions(userId: string): Promise<string[]> {
  const response = await axios.get<string[]>(
    `/api/permission/user/${userId}/codes`
  );
  return response.data;
}

/**
 * Check if role has specified permission
 */
export async function checkRolePermission(
  roleId: string,
  permissionCode: string
): Promise<boolean> {
  const response = await axios.get<boolean>(
    `/api/permission/role/${roleId}/has/${permissionCode}`
  );
  return response.data;
}

/**
 * Create permission
 */
export async function createPermission(
  data: CreatePermissionDto
): Promise<PermissionModel> {
  const response = await axios.post<PermissionModel>('/api/permission', data);
  return response.data;
}

/**
 * Update permission
 */
export async function updatePermission(
  id: string,
  data: UpdatePermissionDto
): Promise<boolean> {
  const response = await axios.put<boolean>(`/api/permission/${id}`, data);
  return response.data;
}

/**
 * Delete permission
 */
export async function deletePermission(id: string): Promise<boolean> {
  const response = await axios.delete<boolean>(`/api/permission/${id}`);
  return response.data;
}

/**
 * Assign permissions to role
 */
export async function assignPermissionsToRole(
  roleId: string,
  permissionIds: string[]
): Promise<boolean> {
  const response = await axios.post<boolean>(
    `/api/permission/role/${roleId}/assign`,
    permissionIds
  );
  return response.data;
}

/**
 * Remove permissions from role
 */
export async function removePermissionsFromRole(
  roleId: string,
  permissionIds: string[]
): Promise<boolean> {
  const response = await axios.delete<boolean>(
    `/api/permission/role/${roleId}/remove`,
    { data: permissionIds }
  );
  return response.data;
}
