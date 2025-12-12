import axios from '@/api/interceptor';
import { IPagedData } from '../paged-data';

export interface ISystemMetadata {
  id?: string | null;
  name: string;
  key: string;
  value: string;
  description: string;
  nonEditable: boolean;
}

export interface ISystemMetadataModel {
  name: string;
  key: string;
  displayName: string;
  type: string;
  description: string;
}

// API 函数 - 现在直接返回 data 字段
export const metadata = {
  // 表单相关
  form: {
    create: (): ISystemMetadata => ({
      id: null,
      name: '',
      key: '',
      value: '',
      description: '',
      nonEditable: false,
    }),

    reset: (target: ISystemMetadata) => {
      Object.assign(target, metadata.form.create());
    },

    isValid: (form: ISystemMetadata): boolean => {
      return !!(form.name?.trim() && form.key?.trim() && form.value?.trim());
    },
  },
  api: {
    // API 方法
    async get(id: string): Promise<ISystemMetadata> {
      const response = await axios.get(
        `/api/system-metadata/get/${id}`
      );
      return response.data;
    },

    async getPagedList(model: any, page: number, size: number): Promise<IPagedData<ISystemMetadata>> {
      const response = await axios.post(
        `/api/system-metadata/get-paged-list?page=${page}&size=${size}`,
        model
      );
      // 响应拦截器返回 {code, message, data: {list, pagination}}
      const { list, pagination } = response.data;
      return {
        items: list,
        totalCount: pagination.total,
        pageIndex: pagination.current,
        pageSize: pagination.pageSize,
      };
    },

    async getAll(): Promise<ISystemMetadata[]> {
      const response = await axios.get(
        '/api/system-metadata/get-all'
      );
      return response.data;
    },

    async create(model: ISystemMetadata): Promise<number> {
      const response = await axios.post(
        '/api/system-metadata/insert',
        model
      );
      return response.data;
    },

    async update(model: ISystemMetadata): Promise<boolean> {
      const response = await axios.put(
        '/api/system-metadata/update',
        model
      );
      return response.data;
    },

    async delete(id: string): Promise<boolean> {
      const response = await axios.delete(
        `/api/system-metadata/delete/${id}`
      );
      return response.data;
    },
  },
};
