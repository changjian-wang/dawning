import axios from '@/api/interceptor';
import type { IPagedData } from '../paged-data';

// 角色模型
export interface RoleModel {
  id?: string;
  name: string;
  displayName: string;
  description?: string;
  isSystem: boolean;
  isActive: boolean;
  permissions: string[];
  createdAt?: string;
  updatedAt?: string;
}

// 创建角色DTO
export interface CreateRoleDto {
  name: string;
  displayName: string;
  description?: string;
  isActive: boolean;
  permissions: string[];
}

// 更新角色DTO
export interface UpdateRoleDto {
  displayName?: string;
  description?: string;
  isActive?: boolean;
  permissions?: string[];
}

// 角色查询参数
export interface RoleQueryParams {
  name?: string;
  displayName?: string;
  isActive?: boolean;
  isSystem?: boolean;
  page?: number;
  pageSize?: number;
}

/**
 * 获取角色列表（分页）
 */
export async function getRoleList(
  params: RoleQueryParams
): Promise<IPagedData<RoleModel>> {
  const response = await axios.get<{ list: RoleModel[]; pagination: any }>(
    '/api/role',
    { params }
  );
  // 响应拦截器返回 {code, message, data: {list, pagination}}
  const { list, pagination } = response.data;
  return {
    items: list,
    totalCount: pagination.total,
    pageIndex: pagination.current,
    pageSize: pagination.pageSize,
  };
}

/**
 * 获取所有活动角色
 */
export async function getAllActiveRoles(): Promise<RoleModel[]> {
  const response = await axios.get<RoleModel[]>('/api/role/all');
  return response.data;
}

/**
 * 根据ID获取角色
 */
export async function getRoleById(id: string): Promise<RoleModel> {
  const response = await axios.get<RoleModel>(`/api/role/${id}`);
  return response.data;
}

/**
 * 根据名称获取角色
 */
export async function getRoleByName(name: string): Promise<RoleModel> {
  const response = await axios.get<RoleModel>(`/api/role/by-name/${name}`);
  return response.data;
}

/**
 * 检查角色名称是否存在
 */
export async function checkRoleName(
  name: string,
  excludeId?: string
): Promise<boolean> {
  const response = await axios.get<boolean>('/api/role/check-name', {
    params: { name, excludeId },
  });
  return response.data;
}

/**
 * 创建角色
 */
export async function createRole(data: CreateRoleDto): Promise<RoleModel> {
  const response = await axios.post<RoleModel>('/api/role', data);
  return response.data;
}

/**
 * 更新角色
 */
export async function updateRole(
  id: string,
  data: UpdateRoleDto
): Promise<RoleModel> {
  const response = await axios.put<RoleModel>(`/api/role/${id}`, data);
  return response.data;
}

/**
 * 删除角色
 */
export async function deleteRole(id: string): Promise<void> {
  await axios.delete<void>(`/api/role/${id}`);
}
