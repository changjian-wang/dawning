import axios from 'axios';
import type { IPagedData } from '../paged-data';

/**
 * 分页查询请求
 */
export interface PagedQueryRequest<T> {
  pageIndex: number;
  pageSize: number;
  model?: T;
}

/**
 * Scope 数据传输对象
 */
export interface ScopeDto {
  id?: string;
  name: string;
  displayName: string;
  description?: string;
  emphasize: boolean;
  required: boolean;
  showInDiscoveryDocument: boolean;
  properties?: Record<string, string>;
  timestamp?: number;
  createdAt?: string;
  updatedAt?: string;
  operatorId?: string;
}

/**
 * Scope 查询模型
 */
export interface ScopeQueryModel {
  name?: string;
  displayName?: string;
  emphasize?: boolean;
  required?: boolean;
}

/**
 * Scope 分页查询请求
 */
export interface ScopePagedQuery extends PagedQueryRequest<ScopeQueryModel> {}

/**
 * 获取所有 Scopes
 */
export function getAllScopes() {
  return axios.get<ScopeDto[]>('/api/openiddict/scope');
}

/**
 * 根据 ID 获取 Scope
 */
export function getScopeById(id: string) {
  return axios.get<ScopeDto>(`/api/openiddict/scope/${id}`);
}

/**
 * 根据名称获取 Scope
 */
export function getScopeByName(name: string) {
  return axios.get<ScopeDto>(`/api/openiddict/scope/by-name/${name}`);
}

/**
 * 根据名称列表批量获取 Scopes
 */
export function getScopesByNames(names: string[]) {
  return axios.post<ScopeDto[]>('/api/openiddict/scope/by-names', names);
}

/**
 * 分页查询 Scopes
 */
export function getScopesPaged(query: ScopePagedQuery) {
  return axios.post<IPagedData<ScopeDto>>('/api/openiddict/scope/paged', query);
}

/**
 * 创建 Scope
 */
export function createScope(data: ScopeDto) {
  return axios.post<ScopeDto>('/api/openiddict/scope', data);
}

/**
 * 更新 Scope
 */
export function updateScope(id: string, data: ScopeDto) {
  return axios.put<ScopeDto>(`/api/openiddict/scope/${id}`, data);
}

/**
 * 删除 Scope
 */
export function deleteScope(id: string) {
  return axios.delete(`/api/openiddict/scope/${id}`);
}
