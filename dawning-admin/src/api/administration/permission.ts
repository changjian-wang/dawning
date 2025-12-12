import axios from '@/api/interceptor';
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
export async function getPermissionList(params: PermissionQueryParams): Promise<IPagedData<PermissionModel>> {
  const response = await axios.get<IPagedData<PermissionModel>>('/api/permission', { params });
  // ApiResponse 包装的数据，拦截器解包后直接是 IPagedData 格式
  return response.data;
}

/**
 * 获取所有启用的权限
 */
export async function getAllActivePermissions(): Promise<PermissionModel[]> {
  const response = await axios.get<PermissionModel[]>('/api/permission/all');
  return response.data;
}

/**
 * 获取分组的权限（按资源分组）
 */
export async function getGroupedPermissions(): Promise<PermissionGroup[]> {
  const response = await axios.get<PermissionGroup[]>('/api/permission/grouped');
  return response.data;
}

/**
 * 根据ID获取权限
 */
export async function getPermissionById(id: string): Promise<PermissionModel> {
  const response = await axios.get<PermissionModel>(`/api/permission/${id}`);
  return response.data;
}

/**
 * 根据代码获取权限
 */
export async function getPermissionByCode(code: string): Promise<PermissionModel> {
  const response = await axios.get<PermissionModel>(`/api/permission/code/${code}`);
  return response.data;
}

/**
 * 获取角色的权限列表
 */
export async function getRolePermissions(roleId: string): Promise<PermissionModel[]> {
  const response = await axios.get<PermissionModel[]>(`/api/permission/role/${roleId}`);
  return response.data;
}

/**
 * 获取用户的所有权限代码（聚合所有角色的权限）
 */
export async function getUserPermissions(userId: string): Promise<string[]> {
  const response = await axios.get<string[]>(`/api/permission/user/${userId}/codes`);
  return response.data;
}

/**
 * 检查角色是否拥有指定权限
 */
export async function checkRolePermission(roleId: string, permissionCode: string): Promise<boolean> {
  const response = await axios.get<boolean>(`/api/permission/role/${roleId}/has/${permissionCode}`);
  return response.data;
}

/**
 * 创建权限
 */
export async function createPermission(data: CreatePermissionDto): Promise<PermissionModel> {
  const response = await axios.post<PermissionModel>('/api/permission', data);
  return response.data;
}

/**
 * 更新权限
 */
export async function updatePermission(id: string, data: UpdatePermissionDto): Promise<boolean> {
  const response = await axios.put<boolean>(`/api/permission/${id}`, data);
  return response.data;
}

/**
 * 删除权限
 */
export async function deletePermission(id: string): Promise<boolean> {
  const response = await axios.delete<boolean>(`/api/permission/${id}`);
  return response.data;
}

/**
 * 为角色分配权限
 */
export async function assignPermissionsToRole(roleId: string, permissionIds: string[]): Promise<boolean> {
  const response = await axios.post<boolean>(
    `/api/permission/role/${roleId}/assign`,
    permissionIds
  );
  return response.data;
}

/**
 * 移除角色的权限
 */
export async function removePermissionsFromRole(roleId: string, permissionIds: string[]): Promise<boolean> {
  const response = await axios.delete<boolean>(
    `/api/permission/role/${roleId}/remove`,
    { data: permissionIds }
  );
  return response.data;
}
