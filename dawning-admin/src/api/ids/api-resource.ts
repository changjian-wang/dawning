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
 * API Resource 数据传输对象
 */
export interface ApiResourceDto {
  id?: string;
  name: string;
  displayName: string;
  description?: string;
  enabled: boolean;
  allowedAccessTokenSigningAlgorithms: string[];
  showInDiscoveryDocument: boolean;
  scopes: string[];
  userClaims: string[];
  properties?: Record<string, string>;
  timestamp?: number;
  createdAt?: string;
  updatedAt?: string;
  operatorId?: string;
}

/**
 * API Resource 查询模型
 */
export interface ApiResourceQueryModel {
  name?: string;
  displayName?: string;
  enabled?: boolean;
  scope?: string;
}

/**
 * API Resource 分页查询请求
 */
export interface ApiResourcePagedQuery extends PagedQueryRequest<ApiResourceQueryModel> {}

/**
 * 获取所有 API Resources
 */
export function getAllApiResources() {
  return axios.get<ApiResourceDto[]>('/api/openiddict/api-resource');
}

/**
 * 根据 ID 获取 API Resource
 */
export function getApiResourceById(id: string) {
  return axios.get<ApiResourceDto>(`/api/openiddict/api-resource/${id}`);
}

/**
 * 根据名称获取 API Resource
 */
export function getApiResourceByName(name: string) {
  return axios.get<ApiResourceDto>(
    `/api/openiddict/api-resource/by-name/${name}`
  );
}

/**
 * 根据名称列表批量获取 API Resources
 */
export function getApiResourcesByNames(names: string[]) {
  return axios.post<ApiResourceDto[]>(
    '/api/openiddict/api-resource/by-names',
    names
  );
}

/**
 * 分页查询 API Resources
 */
export function getApiResourcesPaged(query: ApiResourcePagedQuery) {
  return axios.post<IPagedData<ApiResourceDto>>(
    '/api/openiddict/api-resource/paged',
    query
  );
}

/**
 * 创建 API Resource
 */
export function createApiResource(data: ApiResourceDto) {
  return axios.post<ApiResourceDto>('/api/openiddict/api-resource', data);
}

/**
 * 更新 API Resource
 */
export function updateApiResource(id: string, data: ApiResourceDto) {
  return axios.put<ApiResourceDto>(
    `/api/openiddict/api-resource/${id}`,
    data
  );
}

/**
 * 删除 API Resource
 */
export function deleteApiResource(id: string) {
  return axios.delete(`/api/openiddict/api-resource/${id}`);
}
