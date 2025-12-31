import axios from '@/api/interceptor';
import { IPagedData } from '../paged-data';

/**
 * OpenIddict scope
 */
export interface IScope {
  id?: string;
  name: string;
  displayName?: string;
  description?: string;
  resources?: string[];
  properties?: Record<string, string>;
  timestamp?: number;
  createdAt?: string;
  updatedAt?: string;
}

/**
 * Scope query model
 */
export interface IScopeQuery {
  name?: string;
  displayName?: string;
}

/**
 * Create scope DTO
 */
export interface ICreateScopeDto {
  name: string;
  displayName?: string;
  description?: string;
  resources?: string[];
}

/**
 * Update scope DTO
 */
export interface IUpdateScopeDto extends ICreateScopeDto {
  id: string;
}

export const scope = {
  /**
   * Get scope details
   */
  async get(id: string): Promise<IScope> {
    const response = await axios.get<IScope>(`/api/openiddict/scope/${id}`);
    return response.data;
  },

  /**
   * Get scope by name
   */
  async getByName(name: string): Promise<IScope> {
    const response = await axios.get<IScope>(
      `/api/openiddict/scope/by-name/${name}`
    );
    return response.data;
  },

  /**
   * Get scopes by name list
   */
  async getByNames(names: string[]): Promise<IScope[]> {
    const response = await axios.post<IScope[]>(
      '/api/openiddict/scope/by-names',
      names
    );
    return response.data;
  },

  /**
   * Get paged list
   */
  async getPagedList(
    query: IScopeQuery,
    page = 1,
    size = 10
  ): Promise<IPagedData<IScope>> {
    const response = await axios.post<IPagedData<IScope>>(
      `/api/openiddict/scope/paged?page=${page}&size=${size}`,
      query
    );
    return response.data;
  },

  /**
   * Get all scopes
   */
  async getAll(): Promise<IScope[]> {
    const response = await axios.get<IScope[]>('/api/openiddict/scope');
    return response.data;
  },

  /**
   * Create scope
   */
  async create(dto: ICreateScopeDto): Promise<number> {
    const response = await axios.post<number>('/api/openiddict/scope', {
      name: dto.name,
      displayName: dto.displayName,
      description: dto.description,
      resources: dto.resources || [],
      properties: {},
    });
    return response.data;
  },

  /**
   * Update scope
   */
  async update(dto: IUpdateScopeDto): Promise<number> {
    const response = await axios.put<number>(
      `/api/openiddict/scope/${dto.id}`,
      dto
    );
    return response.data;
  },

  /**
   * Delete scope
   */
  async delete(id: string): Promise<number> {
    const response = await axios.delete<number>(`/api/openiddict/scope/${id}`);
    return response.data;
  },
};
