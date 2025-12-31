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

// Query model
export interface IApiResourceModel {
  name?: string;
  displayName?: string;
  enabled?: boolean;
}

/**
 * Optimization notes:
 * 1. Provides constructor to support initializing object properties.
 * 2. Supports passing partial properties, avoiding manual assignment of all properties.
 * 3. Default values are set via parameter destructuring for more flexibility.
 * 4. Enhanced type safety for easier future extension and maintenance.
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
  // Get paged list
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
    const { items, totalCount, pageIndex, pageSize: size } = response.data;
    return { items, totalCount, pageIndex, pageSize: size };
  },

  // Get details
  async get(id: string): Promise<IApiResource> {
    const response = await axios.get(`/api/openiddict/api-resource/${id}`);
    return response.data;
  },

  // Create
  async create(model: Partial<IApiResource>): Promise<number> {
    const response = await axios.post('/api/openiddict/api-resource', model);
    return response.data;
  },

  // Update
  async update(id: string, model: Partial<IApiResource>): Promise<boolean> {
    const response = await axios.put(
      `/api/openiddict/api-resource/${id}`,
      model
    );
    return response.data;
  },

  // Delete
  async remove(id: string): Promise<boolean> {
    const response = await axios.delete(`/api/openiddict/api-resource/${id}`);
    return response.data;
  },
};
