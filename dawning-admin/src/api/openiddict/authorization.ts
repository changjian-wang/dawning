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
  // 展示用，可从关联查询获取
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
  // 获取分页列表
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

  // 获取详情
  async get(id: string): Promise<IAuthorization> {
    const response = await axios.get(`/api/openiddict/authorization/get/${id}`);
    return response.data;
  },

  // 按用户获取
  async getBySubject(subject: string): Promise<IAuthorization[]> {
    const response = await axios.get(
      `/api/openiddict/authorization/get-by-subject/${subject}`
    );
    return response.data;
  },

  // 按应用获取
  async getByApplicationId(applicationId: string): Promise<IAuthorization[]> {
    const response = await axios.get(
      `/api/openiddict/authorization/get-by-application/${applicationId}`
    );
    return response.data;
  },

  // 撤销授权（删除）
  async revoke(id: string): Promise<boolean> {
    const response = await axios.delete(
      `/api/openiddict/authorization/delete/${id}`
    );
    return response.data;
  },
};
