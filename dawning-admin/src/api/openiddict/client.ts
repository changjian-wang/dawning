import { http } from '../interceptor';
import { IPagedData } from '../paged-data';
import { IClientClaim } from './client-claim';
import { IClientCorsOrigins } from './client-cors-origin';
import { IClientGrantType } from './client-grant-type';
import { IClientIdProviderRestriction } from './client-id-provider-restriction';
import { IClientPostLogoutRedirectUri } from './client-post-logout-redirect-uri';
import { IClientProperty } from './client-property';
import { IClientScope } from './client-scope';
import { IClientSecret } from './client-secret';
import { IClientRedirectUri } from './client-redirect-uri';
import {
  AccessTokenType,
  GrantType,
  GrantTypes,
  RedirectType,
  RedirectTypes,
  TokenExpiration,
  TokenUsage,
} from '../constants';

export interface IClient {
  id: string;
  enabled: boolean;
  clientId: string;
  clientSecrets: IClientSecret[];
  requireClientSecret: boolean;
  clientName: string;
  description: string;
  clientUri: string;
  logoUri: string;
  requireConsent?: boolean;
  allowRememberConsent?: boolean;
  alwaysIncludeUserClaimsInIdToken?: boolean;
  allowedGrantTypes: IClientGrantType[];
  requirePkce: boolean;
  allowPlainTextPkce?: boolean;
  requireRequestObject?: boolean;
  allowAccessTokensViaBrowser?: boolean;
  redirectUris: IClientRedirectUri[];
  postLogoutRedirectUris: IClientPostLogoutRedirectUri[];
  frontChannelLogoutUri: string;
  frontChannelLogoutSessionRequired?: boolean;
  backChannelLogoutUri: string;
  backChannelLogoutSessionRequired?: boolean;
  allowOfflineAccess?: boolean;
  allowedScopes: IClientScope[];
  identityTokenLifetime?: number;
  allowedIdentityTokenSigningAlgorithms?: string;
  accessTokenLifetime?: number;
  authorizationCodeLifetime?: number;
  consentLifetime?: number;
  absoluteRefreshTokenLifetime?: number;
  slidingRefreshTokenLifetime?: number;
  refreshTokenUsage?: TokenUsage;
  updateAccessTokenClaimsOnRefresh?: boolean;
  refreshTokenExpiration?: TokenExpiration;
  accessTokenType?: AccessTokenType;
  identityProviderRestrictions: IClientIdProviderRestriction[];
  includeJwtId?: boolean;
  claims: IClientClaim[];
  alwaysSendClientClaims?: boolean;
  clientClaimsPrefix?: string;
  pairWiseSubjectSalt: string;
  allowedCorsOrigins: IClientCorsOrigins[];
  properties: IClientProperty[];
  userSsoLifetime?: number;
  userCodeType?: string;
  enableLocalLogin: boolean;
  nonEditable: boolean;
}

export interface IClientWithLoginURI extends IClient {
  loginUri: string;
}

export interface IRedirectUri {
  uri: string;
  type: RedirectType;
  isEditing: boolean;
}

export interface IClientModel {
  clientName: string;
  description: string;
  enabled: boolean;
}

// 统一的 client 对象
export const client = {
  // 表单工厂
  form: {
    // 基础客户端
    create: (): IClient => ({
      id: '',
      enabled: false,
      clientId: '',
      clientSecrets: [],
      requireClientSecret: false,
      clientName: '',
      description: '',
      clientUri: '',
      logoUri: '',
      requireConsent: undefined,
      allowRememberConsent: undefined,
      alwaysIncludeUserClaimsInIdToken: undefined,
      allowedGrantTypes: [],
      requirePkce: false,
      allowPlainTextPkce: undefined,
      requireRequestObject: undefined,
      allowAccessTokensViaBrowser: undefined,
      redirectUris: [],
      postLogoutRedirectUris: [],
      frontChannelLogoutUri: '',
      frontChannelLogoutSessionRequired: undefined,
      backChannelLogoutUri: '',
      backChannelLogoutSessionRequired: undefined,
      allowOfflineAccess: undefined,
      allowedScopes: [],
      identityTokenLifetime: 300,
      allowedIdentityTokenSigningAlgorithms: undefined,
      accessTokenLifetime: 3600,
      authorizationCodeLifetime: 300,
      consentLifetime: undefined,
      absoluteRefreshTokenLifetime: 259200,
      slidingRefreshTokenLifetime: 1296000,
      refreshTokenUsage: 1, // TokenUsage.OneTimeOnly
      updateAccessTokenClaimsOnRefresh: undefined,
      refreshTokenExpiration: 0, // TokenExpiration.Sliding
      accessTokenType: 0, // AccessTokenType.Jwt
      identityProviderRestrictions: [],
      includeJwtId: undefined,
      claims: [],
      alwaysSendClientClaims: undefined,
      clientClaimsPrefix: 'client_',
      pairWiseSubjectSalt: '',
      allowedCorsOrigins: [],
      properties: [],
      userSsoLifetime: undefined,
      userCodeType: undefined,
      enableLocalLogin: true,
      nonEditable: false,
    }),

    // 带登录URI的客户端
    createWithLoginURI: (): IClientWithLoginURI => ({
      ...client.form.create(),
      loginUri: '',
    }),

    // SPA客户端预设
    createSpa: (
      overrides: Partial<IClientWithLoginURI> = {}
    ): IClientWithLoginURI => ({
      ...client.form.createWithLoginURI(),
      clientName: 'SPA Application',
      requireClientSecret: false,
      requirePkce: true,
      allowAccessTokensViaBrowser: true,
      accessTokenType: 0, // AccessTokenType.Jwt
      refreshTokenExpiration: 0, // TokenExpiration.Sliding
      refreshTokenUsage: 1, // TokenUsage.OneTimeOnly
      ...overrides,
    }),

    // Web应用客户端预设
    createWebApp: (
      overrides: Partial<IClientWithLoginURI> = {}
    ): IClientWithLoginURI => ({
      ...client.form.createWithLoginURI(),
      clientName: 'Web Application',
      requireClientSecret: true,
      requirePkce: false,
      allowAccessTokensViaBrowser: false,
      accessTokenType: 1, // AccessTokenType.Reference
      refreshTokenExpiration: 1, // TokenExpiration.Absolute
      refreshTokenUsage: 0, // TokenUsage.ReUse
      ...overrides,
    }),

    // API客户端预设
    createApi: (
      overrides: Partial<IClientWithLoginURI> = {}
    ): IClientWithLoginURI => ({
      ...client.form.createWithLoginURI(),
      clientName: 'API Client',
      requireClientSecret: true,
      requirePkce: false,
      allowAccessTokensViaBrowser: false,
      accessTokenType: 0, // AccessTokenType.Jwt
      allowOfflineAccess: false,
      refreshTokenUsage: 1, // TokenUsage.OneTimeOnly
      ...overrides,
    }),

    // 移动应用客户端预设
    createMobile: (
      overrides: Partial<IClientWithLoginURI> = {}
    ): IClientWithLoginURI => ({
      ...client.form.createWithLoginURI(),
      clientName: 'Mobile Application',
      requireClientSecret: false,
      requirePkce: true,
      allowAccessTokensViaBrowser: false,
      accessTokenType: 0, // AccessTokenType.Jwt
      refreshTokenExpiration: 0, // TokenExpiration.Sliding
      refreshTokenUsage: 1, // TokenUsage.OneTimeOnly
      ...overrides,
    }),

    // 重置方法
    reset: (target: IClient) => {
      Object.assign(target, client.form.create());
    },

    resetWithLoginURI: (target: IClientWithLoginURI) => {
      Object.assign(target, client.form.createWithLoginURI());
    },

    // 克隆方法
    clone: (source: IClient): IClient => ({
      ...source,
      id: '',
      clientId: `${source.clientId}_copy`,
      clientName: `${source.clientName} (Copy)`,
    }),

    cloneWithLoginURI: (source: IClientWithLoginURI): IClientWithLoginURI => ({
      ...source,
      id: '',
      clientId: `${source.clientId}_copy`,
      clientName: `${source.clientName} (Copy)`,
      loginUri: '',
    }),

    // 验证方法
    isValid: (form: IClient): boolean => {
      return !!(form.clientName?.trim() && form.clientId?.trim());
    },

    isValidWithLoginURI: (form: IClientWithLoginURI): boolean => {
      return client.form.isValid(form) && !!form.loginUri?.trim();
    },
  },

  // 重定向URI工厂
  redirectUri: {
    create: (): IRedirectUri => ({
      uri: '',
      type: RedirectTypes.Callback,
      isEditing: false,
    }),

    reset: (target: IRedirectUri) => {
      Object.assign(target, client.redirectUri.create());
    },

    isValid: (redirectUri: IRedirectUri): boolean => {
      return !!redirectUri.uri?.trim();
    },
  },

  // 验证工具
  validate: {
    grantType: (grantType: string): grantType is GrantType => {
      return Object.values(GrantTypes).includes(grantType as GrantType);
    },

    redirectType: (redirectType: string): redirectType is RedirectType => {
      return Object.values(RedirectTypes).includes(
        redirectType as RedirectType
      );
    },
  },

  // API 方法
  async get(id: string): Promise<IClient> {
    const response = await http.get<IClient>(`/api/client/get/${id}`);
    return response.data;
  },

  async getPagedList(
    model: IClientModel,
    page: number,
    size: number
  ): Promise<IPagedData<IClient>> {
    const response = await http.post<IPagedData<IClient>>(
      `/api/client/get-paged-list?page=${page}&size=${size}`,
      model
    );
    return response.data;
  },

  async getAll(): Promise<IClient[]> {
    const response = await http.get<IClient[]>('/api/client/get-all');
    return response.data;
  },

  async create(model: IClient): Promise<string> {
    const response = await http.post<string>('/api/client/insert', model);
    return response.data;
  },

  async update(model: IClient): Promise<boolean> {
    const response = await http.put<boolean>('/api/client/update', model);
    return response.data;
  },

  async delete(id: string): Promise<boolean> {
    const response = await http.delete<boolean>(`/api/client/delete/${id}`);
    return response.data;
  },
};
