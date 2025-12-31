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
  // Get paged list
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
    const { items, totalCount, pageIndex, pageSize: size } = response.data;
    return { items, totalCount, pageIndex, pageSize: size };
  },

  // Get details
  async get(id: string): Promise<IIdentityResource> {
    const response = await axios.get(`/api/openiddict/identity-resource/${id}`);
    return response.data;
  },

  // Create
  async create(model: Partial<IIdentityResource>): Promise<number> {
    const response = await axios.post(
      '/api/openiddict/identity-resource',
      model
    );
    return response.data;
  },

  // Update
  async update(
    id: string,
    model: Partial<IIdentityResource>
  ): Promise<boolean> {
    const response = await axios.put(
      `/api/openiddict/identity-resource/${id}`,
      model
    );
    return response.data;
  },

  // Delete
  async remove(id: string): Promise<boolean> {
    const response = await axios.delete(
      `/api/openiddict/identity-resource/${id}`
    );
    return response.data;
  },
};
