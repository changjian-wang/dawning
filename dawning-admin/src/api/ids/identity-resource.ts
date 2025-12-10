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
 * Identity Resource 数据传输对象
 */
export interface IdentityResourceDto {
  id?: string;
  name: string;
  displayName: string;
  description?: string;
  enabled: boolean;
  required: boolean;
  emphasize: boolean;
  showInDiscoveryDocument: boolean;
  userClaims: string[];
  properties?: Record<string, string>;
  timestamp?: number;
  createdAt?: string;
  updatedAt?: string;
  operatorId?: string;
}

/**
 * Identity Resource 查询模型
 */
export interface IdentityResourceQueryModel {
  name?: string;
  displayName?: string;
  enabled?: boolean;
  required?: boolean;
}

/**
 * Identity Resource 分页查询请求
 */
export interface IdentityResourcePagedQuery extends PagedQueryRequest<IdentityResourceQueryModel> {}

/**
 * 获取所有 Identity Resources
 */
export function getAllIdentityResources() {
  return axios.get<IdentityResourceDto[]>(
    '/api/openiddict/identity-resource'
  );
}

/**
 * 根据 ID 获取 Identity Resource
 */
export function getIdentityResourceById(id: string) {
  return axios.get<IdentityResourceDto>(
    `/api/openiddict/identity-resource/${id}`
  );
}

/**
 * 根据名称获取 Identity Resource
 */
export function getIdentityResourceByName(name: string) {
  return axios.get<IdentityResourceDto>(
    `/api/openiddict/identity-resource/by-name/${name}`
  );
}

/**
 * 根据名称列表批量获取 Identity Resources
 */
export function getIdentityResourcesByNames(names: string[]) {
  return axios.post<IdentityResourceDto[]>(
    '/api/openiddict/identity-resource/by-names',
    names
  );
}

/**
 * 分页查询 Identity Resources
 */
export function getIdentityResourcesPaged(query: IdentityResourcePagedQuery) {
  return axios.post<IPagedData<IdentityResourceDto>>(
    '/api/openiddict/identity-resource/paged',
    query
  );
}

/**
 * 创建 Identity Resource
 */
export function createIdentityResource(data: IdentityResourceDto) {
  return axios.post<IdentityResourceDto>(
    '/api/openiddict/identity-resource',
    data
  );
}

/**
 * 更新 Identity Resource
 */
export function updateIdentityResource(id: string, data: IdentityResourceDto) {
  return axios.put<IdentityResourceDto>(
    `/api/openiddict/identity-resource/${id}`,
    data
  );
}

/**
 * 删除 Identity Resource
 */
export function deleteIdentityResource(id: string) {
  return axios.delete(`/api/openiddict/identity-resource/${id}`);
}
