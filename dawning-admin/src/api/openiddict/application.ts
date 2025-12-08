import { http } from '../interceptor';
import { IPagedData } from '../paged-data';

/**
 * OpenIddict 应用程序（客户端）
 */
export interface IApplication {
  id?: string;
  clientId: string;
  clientSecret?: string;
  displayName?: string;
  type?: string; // 'confidential' | 'public'
  consentType?: string; // 'explicit' | 'implicit' | 'systematic'
  permissions?: string[];
  redirectUris?: string[];
  postLogoutRedirectUris?: string[];
  requirements?: string[];
  properties?: Record<string, string>;
  timestamp?: number;
  createdAt?: string;
  updatedAt?: string;
}

/**
 * 应用程序查询模型
 */
export interface IApplicationQuery {
  clientId?: string;
  displayName?: string;
  type?: string;
}

/**
 * 创建应用程序DTO
 */
export interface ICreateApplicationDto {
  clientId: string;
  clientSecret?: string; // Public客户端不需要
  displayName: string;
  type: 'confidential' | 'public';
  consentType: 'explicit' | 'implicit' | 'systematic';
  permissions: string[];
  redirectUris: string[];
  postLogoutRedirectUris: string[];
}

/**
 * 更新应用程序DTO
 */
export interface IUpdateApplicationDto extends ICreateApplicationDto {
  id: string;
}

class ApplicationApi {
  /**
   * 获取应用程序详情
   */
  async get(id: string): Promise<IApplication> {
    const response = await http.get<{ data: IApplication }>(
      `/api/openiddict/application/get/${id}`
    );
    return response.data.data;
  }

  /**
   * 根据ClientId获取应用程序
   */
  async getByClientId(clientId: string): Promise<IApplication> {
    const response = await http.get<{ data: IApplication }>(
      `/api/openiddict/application/get-by-client-id/${clientId}`
    );
    return response.data.data;
  }

  /**
   * 获取分页列表
   */
  async getPagedList(
    query: IApplicationQuery,
    page: number = 1,
    size: number = 10
  ): Promise<IPagedData<IApplication>> {
    const response = await http.post<{ data: IPagedData<IApplication> }>(
      `/api/openiddict/application/get-paged-list?page=${page}&size=${size}`,
      query
    );
    return response.data.data;
  }

  /**
   * 获取所有应用程序
   */
  async getAll(): Promise<IApplication[]> {
    const response = await http.get<{ data: IApplication[] }>(
      '/api/openiddict/application/get-all'
    );
    return response.data.data;
  }

  /**
   * 创建应用程序
   */
  async create(dto: ICreateApplicationDto): Promise<number> {
    const response = await http.post<{ data: number }>(
      '/api/openiddict/application/insert',
      {
        clientId: dto.clientId,
        clientSecret: dto.clientSecret,
        displayName: dto.displayName,
        type: dto.type,
        consentType: dto.consentType,
        permissions: dto.permissions,
        redirectUris: dto.redirectUris,
        postLogoutRedirectUris: dto.postLogoutRedirectUris,
        requirements: [],
        properties: {},
      }
    );
    return response.data.data;
  }

  /**
   * 更新应用程序
   */
  async update(dto: IUpdateApplicationDto): Promise<number> {
    const response = await http.put<{ data: number }>(
      '/api/openiddict/application/update',
      dto
    );
    return response.data.data;
  }

  /**
   * 删除应用程序
   */
  async delete(id: string): Promise<number> {
    const response = await http.delete<{ data: number }>(
      `/api/openiddict/application/delete/${id}`
    );
    return response.data.data;
  }

  /**
   * 更新客户端密钥
   */
  async updateSecret(id: string, newSecret: string): Promise<number> {
    const response = await http.post<{ data: number }>(
      '/api/openiddict/application/update-secret',
      { id, newSecret }
    );
    return response.data.data;
  }
}

export const application = new ApplicationApi();
