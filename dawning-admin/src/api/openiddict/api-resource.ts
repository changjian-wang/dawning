import axios from '@/api/interceptor';
import { IPagedData } from '@/api/paged-data';
import { IApiResourceClaim } from './api-resource-claim';
import { IApiResourceProperty } from './api-resource-property';
import { IApiResourceScope } from './api-resource-scope';
import { IApiResourceSecret } from './api-resource-secret';

export interface IApiResource {
  id: string;
  enabled: boolean;
  name: string;
  displayName: string;
  description: string;
  allowedAccessTokenSigningAlgorithms: string;
  showInDiscoveryDocument: boolean;
  secrets: IApiResourceSecret[];
  scopes: IApiResourceScope[];
  claims: IApiResourceClaim[];
  properties: IApiResourceProperty[];
  nonEditable: boolean;
}

// 查询模型
export interface IApiResourceModel {
  name?: string;
  displayName?: string;
  enabled?: boolean;
}

/**
 * 优化说明:
 * 1. 提供构造函数，支持初始化对象属性。
 * 2. 支持传入部分属性，避免手动赋值全部属性。
 * 3. 默认值通过参数解构设置，更灵活。
 * 4. 增加类型安全，方便后续拓展和维护。
 */
export class ApiResource implements IApiResource {
  id: string;
  enabled: boolean;
  name: string;
  displayName: string;
  description: string;
  allowedAccessTokenSigningAlgorithms: string;
  showInDiscoveryDocument: boolean;
  secrets: IApiResourceSecret[];
  scopes: IApiResourceScope[];
  claims: IApiResourceClaim[];
  properties: IApiResourceProperty[];
  nonEditable: boolean;

  constructor({
    id = '',
    enabled = false,
    name = '',
    displayName = '',
    description = '',
    allowedAccessTokenSigningAlgorithms = '',
    showInDiscoveryDocument = true,
    secrets = [],
    scopes = [],
    claims = [],
    properties = [],
    nonEditable = false,
  }: Partial<IApiResource> = {}) {
    this.id = id;
    this.enabled = enabled;
    this.name = name;
    this.displayName = displayName;
    this.description = description;
    this.allowedAccessTokenSigningAlgorithms =
      allowedAccessTokenSigningAlgorithms;
    this.showInDiscoveryDocument = showInDiscoveryDocument;
    this.secrets = secrets;
    this.scopes = scopes;
    this.claims = claims;
    this.properties = properties;
    this.nonEditable = nonEditable;
  }
}

export const apiResourceApi = {
  // 获取分页列表
  async getPagedList(
    model: IApiResourceModel,
    page: number,
    pageSize: number
  ): Promise<IPagedData<IApiResource>> {
    const response = await axios.post(
      '/api/openiddict/api-resource/paged',
      model,
      {
        params: { page, pageSize },
      }
    );
    const { items, totalCount, pageIndex, pageSize: size } = response.data.data;
    return { items, totalCount, pageIndex, pageSize: size };
  },

  // 获取详情
  async get(id: string): Promise<IApiResource> {
    const response = await axios.get(`/api/openiddict/api-resource/${id}`);
    return response.data.data;
  },

  // 创建
  async create(model: Partial<IApiResource>): Promise<number> {
    const response = await axios.post('/api/openiddict/api-resource', model);
    return response.data.data;
  },

  // 更新
  async update(id: string, model: Partial<IApiResource>): Promise<boolean> {
    const response = await axios.put(
      `/api/openiddict/api-resource/${id}`,
      model
    );
    return response.data.data;
  },

  // 删除
  async remove(id: string): Promise<boolean> {
    const response = await axios.delete(`/api/openiddict/api-resource/${id}`);
    return response.data.data;
  },
};
