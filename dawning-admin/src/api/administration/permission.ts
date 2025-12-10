import axios from 'axios';
import type { IPagedData } from '../paged-data';

/**
 * 权限模型
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
 * 创建权限DTO
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
 * 更新权限DTO
 */
export interface UpdatePermissionDto {
  name?: string;
  description?: string;
  isActive?: boolean;
  displayOrder?: number;
}

/**
 * 权限查询参数
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
 * 角色权限关联
 */
export interface RolePermissionDto {
  roleId: string;
  permissionIds: string[];
}

/**
 * 权限分组（按资源）
 */
export interface PermissionGroup {
  resource: string;
  permissions: PermissionModel[];
}

/**
 * 获取权限列表（分页）
 */
export function getPermissionList(params: PermissionQueryParams) {
  return axios.get<IPagedData<PermissionModel>>('/api/permission', { params });
}

/**
 * 获取所有启用的权限
 */
export function getAllActivePermissions() {
  return axios.get<PermissionModel[]>('/api/permission/all');
}

/**
 * 获取分组的权限（按资源分组）
 */
export function getGroupedPermissions() {
  return axios.get<PermissionGroup[]>('/api/permission/grouped');
}

/**
 * 根据ID获取权限
 */
export function getPermissionById(id: string) {
  return axios.get<PermissionModel>(`/api/permission/${id}`);
}

/**
 * 根据代码获取权限
 */
export function getPermissionByCode(code: string) {
  return axios.get<PermissionModel>(`/api/permission/code/${code}`);
}

/**
 * 获取角色的权限列表
 */
export function getRolePermissions(roleId: string) {
  return axios.get<PermissionModel[]>(`/api/permission/role/${roleId}`);
}

/**
 * 获取用户的所有权限代码（聚合所有角色的权限）
 */
export function getUserPermissions(userId: string) {
  return axios.get<string[]>(`/api/permission/user/${userId}/codes`);
}

/**
 * 检查角色是否拥有指定权限
 */
export function checkRolePermission(roleId: string, permissionCode: string) {
  return axios.get<boolean>(`/api/permission/role/${roleId}/has/${permissionCode}`);
}

/**
 * 创建权限
 */
export function createPermission(data: CreatePermissionDto) {
  return axios.post<{ data: PermissionModel; message: string }>('/api/permission', data);
}

/**
 * 更新权限
 */
export function updatePermission(id: string, data: UpdatePermissionDto) {
  return axios.put<{ data: boolean; message: string }>(`/api/permission/${id}`, data);
}

/**
 * 删除权限
 */
export function deletePermission(id: string) {
  return axios.delete<{ data: boolean; message: string }>(`/api/permission/${id}`);
}

/**
 * 为角色分配权限
 */
export function assignPermissionsToRole(roleId: string, permissionIds: string[]) {
  return axios.post<{ data: boolean; message: string }>(
    `/api/permission/role/${roleId}/assign`,
    permissionIds
  );
}

/**
 * 移除角色的权限
 */
export function removePermissionsFromRole(roleId: string, permissionIds: string[]) {
  return axios.delete<{ data: boolean; message: string }>(
    `/api/permission/role/${roleId}/remove`,
    { data: permissionIds }
  );
}
