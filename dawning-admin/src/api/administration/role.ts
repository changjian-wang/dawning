import axios from 'axios';
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
export function getRoleList(params: RoleQueryParams) {
  return axios.get<any>('/api/role', { params });
}

/**
 * 获取所有活动角色
 */
export function getAllActiveRoles() {
  return axios.get<RoleModel[]>('/api/role/all');
}

/**
 * 根据ID获取角色
 */
export function getRoleById(id: string) {
  return axios.get<RoleModel>(`/api/role/${id}`);
}

/**
 * 根据名称获取角色
 */
export function getRoleByName(name: string) {
  return axios.get<RoleModel>(`/api/role/by-name/${name}`);
}

/**
 * 检查角色名称是否存在
 */
export function checkRoleName(name: string, excludeId?: string) {
  return axios.get<boolean>('/api/role/check-name', {
    params: { name, excludeId },
  });
}

/**
 * 创建角色
 */
export function createRole(data: CreateRoleDto) {
  return axios.post<RoleModel>('/api/role', data);
}

/**
 * 更新角色
 */
export function updateRole(id: string, data: UpdateRoleDto) {
  return axios.put<RoleModel>(`/api/role/${id}`, data);
}

/**
 * 删除角色
 */
export function deleteRole(id: string) {
  return axios.delete(`/api/role/${id}`);
}
