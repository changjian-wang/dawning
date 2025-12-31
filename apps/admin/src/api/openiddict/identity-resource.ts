import axios from '@/api/interceptor';
import { IPagedData } from '../paged-data';

export interface IIdentityResourceClaim {
  id?: string | null;
  type: string;
  identityResourceId: string;
  createdBy?: string;
  createdAt?: string;
  updatedBy?: string;
  updatedAt?: string;
}

export interface IIdentityResourceClaimModel {
  type: string;
  identityResourceId: string;
}

// Unified identityResourceClaim object
export const identityResourceClaim = {
  // Form factory
  form: {
    create: (identityResourceId?: string): IIdentityResourceClaim => ({
      id: null,
      type: '',
      identityResourceId: identityResourceId || '',
    }),

    reset: (target: IIdentityResourceClaim) => {
      Object.assign(
        target,
        identityResourceClaim.form.create(target.identityResourceId)
      );
    },

    clone: (source: IIdentityResourceClaim): IIdentityResourceClaim => ({
      ...source,
      id: null,
    }),

    isValid: (form: IIdentityResourceClaim): boolean => {
      return !!(form.type?.trim() && form.identityResourceId?.trim());
    },
  },

  // API methods
  async get(id: string): Promise<IIdentityResourceClaim> {
    const response = await axios.get<IIdentityResourceClaim>(
      `/api/identity-resource-claim/get/${id}`
    );
    return response.data;
  },

  async getByIdentityResourceId(
    identityResourceId: string
  ): Promise<IIdentityResourceClaim[]> {
    const response = await axios.get<IIdentityResourceClaim[]>(
      `/api/identity-resource-claim/get-by-resource/${identityResourceId}`
    );
    return response.data;
  },

  async getPagedList(
    model: IIdentityResourceClaimModel,
    page: number,
    size: number
  ): Promise<IPagedData<IIdentityResourceClaim>> {
    const response = await axios.post<IPagedData<IIdentityResourceClaim>>(
      `/api/identity-resource-claim/get-paged-list?page=${page}&size=${size}`,
      model
    );
    return response.data;
  },

  async create(model: IIdentityResourceClaim): Promise<number> {
    const response = await axios.post<number>(
      '/api/identity-resource-claim/insert',
      model
    );
    return response.data;
  },

  async update(model: IIdentityResourceClaim): Promise<boolean> {
    const response = await axios.put<boolean>(
      '/api/identity-resource-claim/update',
      model
    );
    return response.data;
  },

  async delete(id: string): Promise<boolean> {
    const response = await axios.delete<boolean>(
      `/api/identity-resource-claim/delete/${id}`
    );
    return response.data;
  },

  async deleteByIdentityResourceId(
    identityResourceId: string
  ): Promise<boolean> {
    const response = await axios.delete<boolean>(
      `/api/identity-resource-claim/delete-by-resource/${identityResourceId}`
    );
    return response.data;
  },
};
