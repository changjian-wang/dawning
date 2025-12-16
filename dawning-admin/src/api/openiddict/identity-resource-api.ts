import axios from '@/api/interceptor';
import { IPagedData } from '@/api/paged-data';

export interface IIdentityResource {
  id: string;
  name: string;
  displayName: string;
  description: string;
  enabled: boolean;
  required: boolean;
  emphasize: boolean;
  showInDiscoveryDocument: boolean;
  userClaims: string[];
  properties: Record<string, string>;
  createdAt?: string;
  updatedAt?: string;
}

export interface IIdentityResourceModel {
  name?: string;
  displayName?: string;
  enabled?: boolean;
}

export const identityResourceApi = {
  // 获取分页列表
  async getPagedList(
    model: IIdentityResourceModel,
    page: number,
    pageSize: number
  ): Promise<IPagedData<IIdentityResource>> {
    const response = await axios.post(
      '/api/openiddict/identity-resource/paged',
      model,
      {
        params: { page, pageSize },
      }
    );
    const { items, totalCount, pageIndex, pageSize: size } = response.data.data;
    return { items, totalCount, pageIndex, pageSize: size };
  },

  // 获取详情
  async get(id: string): Promise<IIdentityResource> {
    const response = await axios.get(`/api/openiddict/identity-resource/${id}`);
    return response.data.data;
  },

  // 创建
  async create(model: Partial<IIdentityResource>): Promise<number> {
    const response = await axios.post(
      '/api/openiddict/identity-resource',
      model
    );
    return response.data.data;
  },

  // 更新
  async update(
    id: string,
    model: Partial<IIdentityResource>
  ): Promise<boolean> {
    const response = await axios.put(
      `/api/openiddict/identity-resource/${id}`,
      model
    );
    return response.data.data;
  },

  // 删除
  async remove(id: string): Promise<boolean> {
    const response = await axios.delete(
      `/api/openiddict/identity-resource/${id}`
    );
    return response.data.data;
  },
};
