import axios from '@/api/interceptor';
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

/**
 * 客户端类型预设
 */
export const ClientTypePresets = {
  /**
   * SPA应用 (React, Vue, Angular)
   */
  spa: (): Partial<ICreateApplicationDto> => ({
    type: 'public',
    consentType: 'explicit',
    permissions: [
      'ept:authorization',
      'ept:token',
      'ept:logout',
      'gt:authorization_code',
      'gt:refresh_token',
      'rst:code',
      'scp:openid',
      'scp:profile',
      'scp:email',
    ],
    redirectUris: [],
    postLogoutRedirectUris: [],
  }),

  /**
   * 移动应用 (iOS, Android)
   */
  mobile: (): Partial<ICreateApplicationDto> => ({
    type: 'public',
    consentType: 'explicit',
    permissions: [
      'ept:authorization',
      'ept:token',
      'ept:logout',
      'gt:authorization_code',
      'gt:refresh_token',
      'rst:code',
      'scp:openid',
      'scp:profile',
      'scp:offline_access',
    ],
    redirectUris: [],
    postLogoutRedirectUris: [],
  }),

  /**
   * Web应用 (传统服务器端渲染)
   */
  web: (): Partial<ICreateApplicationDto> => ({
    type: 'confidential',
    consentType: 'explicit',
    permissions: [
      'ept:authorization',
      'ept:token',
      'ept:logout',
      'gt:authorization_code',
      'gt:refresh_token',
      'rst:code',
      'scp:openid',
      'scp:profile',
      'scp:email',
    ],
    redirectUris: [],
    postLogoutRedirectUris: [],
  }),

  /**
   * API客户端 (机器对机器)
   */
  api: (): Partial<ICreateApplicationDto> => ({
    type: 'confidential',
    consentType: 'implicit',
    permissions: [
      'ept:token',
      'gt:client_credentials',
      'scp:api.read',
      'scp:api.write',
    ],
    redirectUris: [],
    postLogoutRedirectUris: [],
  }),
};

/**
 * OpenIddict权限类型
 */
export const PermissionTypes = {
  // 端点权限
  Endpoints: {
    authorization: 'ept:authorization',
    token: 'ept:token',
    logout: 'ept:logout',
    introspection: 'ept:introspection',
    revocation: 'ept:revocation',
  },
  // 授权类型
  GrantTypes: {
    authorizationCode: 'gt:authorization_code',
    clientCredentials: 'gt:client_credentials',
    refreshToken: 'gt:refresh_token',
    implicit: 'gt:implicit',
    password: 'gt:password',
  },
  // 响应类型
  ResponseTypes: {
    code: 'rst:code',
    token: 'rst:token',
    idToken: 'rst:id_token',
  },
  // 作用域
  Scopes: {
    openid: 'scp:openid',
    profile: 'scp:profile',
    email: 'scp:email',
    roles: 'scp:roles',
    offlineAccess: 'scp:offline_access',
  },
};

export const application = {
  /**
   * 获取应用程序详情
   */
  async get(id: string): Promise<IApplication> {
    const response = await axios.get<IApplication>(`/api/openiddict/application/${id}`);
    return response.data;
  },

  /**
   * 根据ClientId获取应用程序
   */
  async getByClientId(clientId: string): Promise<IApplication> {
    const response = await axios.get<IApplication>(
      `/api/openiddict/application/by-client-id/${clientId}`
    );
    return response.data;
  },

  /**
   * 获取分页列表
   */
  async getPagedList(
    query: IApplicationQuery,
    page = 1,
    size = 10
  ): Promise<IPagedData<IApplication>> {
    const response = await axios.post<IPagedData<IApplication>>(
      `/api/openiddict/application/paged?page=${page}&size=${size}`,
      query
    );
    return response.data;
  },

  /**
   * 获取所有应用程序
   */
  async getAll(): Promise<IApplication[]> {
    const response = await axios.get<IApplication[]>('/api/openiddict/application');
    return response.data;
  },

  /**
   * 创建应用程序
   */
  async create(dto: ICreateApplicationDto): Promise<string> {
    const response = await axios.post<string>('/api/openiddict/application', {
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
    });
    return response.data;
  },

  /**
   * 更新应用程序
   */
  async update(dto: IUpdateApplicationDto): Promise<boolean> {
    const response = await axios.put<boolean>(
      `/api/openiddict/application/${dto.id}`,
      dto
    );
    return response.data;
  },

  /**
   * 删除应用程序
   */
  async delete(id: string): Promise<boolean> {
    const response = await axios.delete<boolean>(`/api/openiddict/application/${id}`);
    return response.data;
  },

  /**
   * 更新客户端密钥（重新生成哈希）
   */
  async updateSecret(id: string, newSecret: string): Promise<boolean> {
    // 通过update接口更新密钥
    const app = await this.get(id);
    return this.update({
      ...app,
      id,
      clientSecret: newSecret,
    } as IUpdateApplicationDto);
  },
};
