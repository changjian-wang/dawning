import axios from '@/api/interceptor';
import type { IPagedData } from '../paged-data';

// Role model
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

// Create role DTO
export interface CreateRoleDto {
  name: string;
  displayName: string;
  description?: string;
  isActive: boolean;
  permissions: string[];
}

// Update role DTO
export interface UpdateRoleDto {
  displayName?: string;
  description?: string;
  isActive?: boolean;
  permissions?: string[];
}

// Role query parameters
export interface RoleQueryParams {
  name?: string;
  displayName?: string;
  isActive?: boolean;
  isSystem?: boolean;
  page?: number;
  pageSize?: number;
}

/**
 * Get role list (paginated)
 */
export async function getRoleList(
  params: RoleQueryParams
): Promise<IPagedData<RoleModel>> {
  const response = await axios.get<{ list: RoleModel[]; pagination: any }>(
    '/api/role',
    { params }
  );
  // Response interceptor returns {code, message, data: {list, pagination}}
  const { list, pagination } = response.data;
  return {
    items: list,
    totalCount: pagination.total,
    pageIndex: pagination.current,
    pageSize: pagination.pageSize,
  };
}

/**
 * Get all active roles
 */
export async function getAllActiveRoles(): Promise<RoleModel[]> {
  const response = await axios.get<RoleModel[]>('/api/role/all');
  return response.data;
}

/**
 * Get role by ID
 */
export async function getRoleById(id: string): Promise<RoleModel> {
  const response = await axios.get<RoleModel>(`/api/role/${id}`);
  return response.data;
}

/**
 * Get role by name
 */
export async function getRoleByName(name: string): Promise<RoleModel> {
  const response = await axios.get<RoleModel>(`/api/role/by-name/${name}`);
  return response.data;
}

/**
 * Check if role name exists
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
 * Create role
 */
export async function createRole(data: CreateRoleDto): Promise<RoleModel> {
  const response = await axios.post<RoleModel>('/api/role', data);
  return response.data;
}

/**
 * Update role
 */
export async function updateRole(
  id: string,
  data: UpdateRoleDto
): Promise<RoleModel> {
  const response = await axios.put<RoleModel>(`/api/role/${id}`, data);
  return response.data;
}

/**
 * Delete role
 */
export async function deleteRole(id: string): Promise<void> {
  await axios.delete<void>(`/api/role/${id}`);
}
