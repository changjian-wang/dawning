import axios from '@/api/interceptor';
import { IPagedData } from '../paged-data';

/**
 * OpenIddict 作用域
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
}

/**
 * 作用域查询模型
 */
export interface IScopeQuery {
  name?: string;
  displayName?: string;
}

/**
 * 创建作用域DTO
 */
export interface ICreateScopeDto {
  name: string;
  displayName?: string;
  description?: string;
  resources?: string[];
}

/**
 * 更新作用域DTO
 */
export interface IUpdateScopeDto extends ICreateScopeDto {
  id: string;
}

export const scope = {
  /**
   * 获取作用域详情
   */
  async get(id: string): Promise<IScope> {
    const response = await axios.get<IScope>(`/api/openiddict/scope/${id}`);
    return response.data;
  },

  /**
   * 根据名称获取作用域
   */
  async getByName(name: string): Promise<IScope> {
    const response = await axios.get<IScope>(
      `/api/openiddict/scope/by-name/${name}`
    );
    return response.data;
  },

  /**
   * 根据名称列表获取作用域
   */
  async getByNames(names: string[]): Promise<IScope[]> {
    const response = await axios.post<IScope[]>(
      '/api/openiddict/scope/by-names',
      names
    );
    return response.data;
  },

  /**
   * 获取分页列表
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
   * 获取所有作用域
   */
  async getAll(): Promise<IScope[]> {
    const response = await axios.get<IScope[]>('/api/openiddict/scope');
    return response.data;
  },

  /**
   * 创建作用域
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
   * 更新作用域
   */
  async update(dto: IUpdateScopeDto): Promise<number> {
    const response = await axios.put<number>(`/api/openiddict/scope/${dto.id}`, dto);
    return response.data;
  },

  /**
   * 删除作用域
   */
  async delete(id: string): Promise<number> {
    const response = await axios.delete<number>(`/api/openiddict/scope/${id}`);
    return response.data;
  },
};
