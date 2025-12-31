import axios from '@/api/interceptor';
import { IPagedData } from '@/api/paged-data';

export interface IAuthorization {
  id: string;
  applicationId?: string;
  subject?: string;
  type?: string;
  status?: string;
  scopes: string[];
  properties: Record<string, string>;
  createdAt?: string;
  // For display, can be obtained from join query
  applicationName?: string;
  userName?: string;
}

export interface IAuthorizationModel {
  subject?: string;
  applicationId?: string;
  status?: string;
  type?: string;
}

export const authorizationApi = {
  // Get paged list
  async getPagedList(
    model: IAuthorizationModel,
    page: number,
    pageSize: number
  ): Promise<IPagedData<IAuthorization>> {
    const response = await axios.post(
      '/api/openiddict/authorization/get-paged-list',
      model,
      {
        params: { page, pageSize },
      }
    );
    const { items, totalCount, pageIndex, pageSize: size } = response.data;
    return { items, totalCount, pageIndex, pageSize: size };
  },

  // Get details
  async get(id: string): Promise<IAuthorization> {
    const response = await axios.get(`/api/openiddict/authorization/get/${id}`);
    return response.data;
  },

  // Get by user
  async getBySubject(subject: string): Promise<IAuthorization[]> {
    const response = await axios.get(
      `/api/openiddict/authorization/get-by-subject/${subject}`
    );
    return response.data;
  },

  // Get by application
  async getByApplicationId(applicationId: string): Promise<IAuthorization[]> {
    const response = await axios.get(
      `/api/openiddict/authorization/get-by-application/${applicationId}`
    );
    return response.data;
  },

  // Revoke authorization (delete)
  async revoke(id: string): Promise<boolean> {
    const response = await axios.delete(
      `/api/openiddict/authorization/delete/${id}`
    );
    return response.data;
  },
};
