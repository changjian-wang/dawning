import { http } from '../interceptor';
import { IPagedData } from '../paged-data';

export interface IClaimType {
  id?: string | null;
  name: string;
  displayName: string;
  type: string;
  description: string;
  required: boolean;
  nonEditable: boolean;
  createdBy?: string;
  createdAt?: string;
  updatedBy?: string;
  updatedAt?: string;
}

export interface IClaimTypeModel {
  name: string;
  displayName: string;
  type: string;
  description: string;
}

// 统一的 claimType 对象
export const claimType = {
  // 表单工厂
  form: {
    create: (): IClaimType => ({
      id: null,
      name: '',
      displayName: '',
      type: '',
      description: '',
      required: false,
      nonEditable: false,
    }),

    reset: (target: IClaimType) => {
      Object.assign(target, claimType.form.create());
    },

    clone: (source: IClaimType): IClaimType => ({
      ...source,
      id: null,
      name: `${source.name} (Copy)`,
      displayName: `${source.displayName} (Copy)`,
    }),

    isValid: (form: IClaimType): boolean => {
      return !!(
        form.name?.trim() &&
        form.displayName?.trim() &&
        form.type?.trim()
      );
    },
  },
  api: {
    // API 方法
    async get(id: string): Promise<IClaimType> {
      const response = await http.get<IClaimType>(`/api/claim-type/get/${id}`);
      return response.data;
    },

    async getPagedList(
      model: IClaimTypeModel,
      page: number,
      size: number
    ): Promise<IPagedData<IClaimType>> {
      const response = await http.post<IPagedData<IClaimType>>(
        `/api/claim-type/get-paged-list?page=${page}&size=${size}`,
        model
      );
      return response.data;
    },

    async getAll(): Promise<IClaimType[]> {
      const response = await http.get<IClaimType[]>('/api/claim-type/get-all');
      return response.data;
    },

    async create(model: IClaimType): Promise<number> {
      const response = await http.post<number>('/api/claim-type/insert', model);
      return response.data;
    },

    async update(model: IClaimType): Promise<boolean> {
      const response = await http.put<boolean>('/api/claim-type/update', model);
      return response.data;
    },

    async delete(id: string): Promise<boolean> {
      const response = await http.delete<boolean>(
        `/api/claim-type/delete/${id}`
      );
      return response.data;
    },
  },
};
