import axios from '@/api/interceptor';
import { IPagedData } from '../paged-data';

/**
 * OpenIddict Application (Client)
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
 * Application query model
 */
export interface IApplicationQuery {
  clientId?: string;
  displayName?: string;
  type?: string;
}

/**
 * Create application DTO
 */
export interface ICreateApplicationDto {
  clientId: string;
  clientSecret?: string; // Not required for Public clients
  displayName: string;
  type: 'confidential' | 'public';
  consentType: 'explicit' | 'implicit' | 'systematic';
  permissions: string[];
  redirectUris: string[];
  postLogoutRedirectUris: string[];
}

/**
 * Update application DTO
 */
export interface IUpdateApplicationDto extends ICreateApplicationDto {
  id: string;
}

/**
 * Client type presets
 */
export const ClientTypePresets = {
  /**
   * SPA Application (React, Vue, Angular)
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
   * Mobile Application (iOS, Android)
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
   * Web Application (Traditional server-side rendering)
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
   * API Client (Machine-to-machine)
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
 * OpenIddict Permission Types
 */
export const PermissionTypes = {
  // Endpoint permissions
  Endpoints: {
    authorization: 'ept:authorization',
    token: 'ept:token',
    logout: 'ept:logout',
    introspection: 'ept:introspection',
    revocation: 'ept:revocation',
  },
  // Grant types
  GrantTypes: {
    authorizationCode: 'gt:authorization_code',
    clientCredentials: 'gt:client_credentials',
    refreshToken: 'gt:refresh_token',
    implicit: 'gt:implicit',
    password: 'gt:password',
  },
  // Response types
  ResponseTypes: {
    code: 'rst:code',
    token: 'rst:token',
    idToken: 'rst:id_token',
  },
  // Scopes
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
   * Get application details
   */
  async get(id: string): Promise<IApplication> {
    const response = await axios.get<IApplication>(
      `/api/openiddict/application/${id}`
    );
    return response.data;
  },

  /**
   * Get application by ClientId
   */
  async getByClientId(clientId: string): Promise<IApplication> {
    const response = await axios.get<IApplication>(
      `/api/openiddict/application/by-client-id/${clientId}`
    );
    return response.data;
  },

  /**
   * Get paged list
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
   * Get all applications
   */
  async getAll(): Promise<IApplication[]> {
    const response = await axios.get<IApplication[]>(
      '/api/openiddict/application'
    );
    return response.data;
  },

  /**
   * Create application
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
   * Update application
   */
  async update(dto: IUpdateApplicationDto): Promise<boolean> {
    const response = await axios.put<boolean>(
      `/api/openiddict/application/${dto.id}`,
      dto
    );
    return response.data;
  },

  /**
   * Delete application
   */
  async delete(id: string): Promise<boolean> {
    const response = await axios.delete<boolean>(
      `/api/openiddict/application/${id}`
    );
    return response.data;
  },

  /**
   * Update client secret (regenerate hash)
   */
  async updateSecret(id: string, newSecret: string): Promise<boolean> {
    // Update secret through update interface
    const app = await this.get(id);
    return this.update({
      ...app,
      id,
      clientSecret: newSecret,
    } as IUpdateApplicationDto);
  },
};
