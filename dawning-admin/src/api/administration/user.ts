import { http } from '../interceptor';
import { IPagedData } from '../paged-data';

export interface IUser {
  id?: string | null;
  userName: string;
  normalizedUserName: string;
  email: string;
  normalizedEmail: string;
  emailConfirmed: boolean;
  phoneNumber: string;
  phoneNumberConfirmed: boolean;
  twoFactorEnabled: boolean;
  lockoutEnabled: boolean;
  lockoutEnd?: string | null;
  accessFailedCount: number;
  timestamp?: number;
  createdBy?: string;
  createdAt?: string;
  updatedBy?: string;
  updatedAt?: string;
}

export interface IUserModel {
  userName?: string;
  email?: string;
  phoneNumber?: string;
  emailConfirmed?: boolean;
  lockoutEnabled?: boolean;
}

export interface ICreateUserModel {
  userName: string;
  email: string;
  phoneNumber?: string;
  password: string;
  emailConfirmed?: boolean;
  phoneNumberConfirmed?: boolean;
  twoFactorEnabled?: boolean;
  lockoutEnabled?: boolean;
}

export interface IUpdateUserModel {
  id: string;
  userName: string;
  email: string;
  phoneNumber?: string;
  emailConfirmed?: boolean;
  phoneNumberConfirmed?: boolean;
  twoFactorEnabled?: boolean;
  lockoutEnabled?: boolean;
}

// 统一的 user 对象
export const user = {
  // 表单工厂
  form: {
    create: (): ICreateUserModel => ({
      userName: '',
      email: '',
      phoneNumber: '',
      password: '',
      emailConfirmed: false,
      phoneNumberConfirmed: false,
      twoFactorEnabled: false,
      lockoutEnabled: false,
    }),

    reset: (target: ICreateUserModel) => {
      Object.assign(target, user.form.create());
    },

    clone: (source: IUser): IUpdateUserModel => ({
      id: source.id || '',
      userName: source.userName,
      email: source.email,
      phoneNumber: source.phoneNumber,
      emailConfirmed: source.emailConfirmed,
      phoneNumberConfirmed: source.phoneNumberConfirmed,
      twoFactorEnabled: source.twoFactorEnabled,
      lockoutEnabled: source.lockoutEnabled,
    }),

    isValid: (form: ICreateUserModel | IUpdateUserModel): boolean => {
      return !!(
        form.userName?.trim() &&
        form.email?.trim() &&
        /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)
      );
    },
  },
  
  api: {
    // API 方法
    async get(id: string): Promise<IUser> {
      const response = await http.get<IUser>(`/api/user/${id}`);
      return response.data;
    },

    async getPagedList(
      model: IUserModel,
      page: number,
      size: number
    ): Promise<IPagedData<IUser>> {
      const response = await http.get<IPagedData<IUser>>(
        `/api/user`,
        {
          params: {
            page,
            size,
            ...model,
          },
        }
      );
      return response.data;
    },

    async getAll(): Promise<IUser[]> {
      const response = await http.get<IUser[]>('/api/user/all');
      return response.data;
    },

    async create(model: ICreateUserModel): Promise<string> {
      const response = await http.post<string>('/api/user', model);
      return response.data;
    },

    async update(model: IUpdateUserModel): Promise<boolean> {
      const response = await http.put<boolean>(`/api/user/${model.id}`, model);
      return response.data;
    },

    async delete(id: string): Promise<boolean> {
      const response = await http.delete<boolean>(`/api/user/${id}`);
      return response.data;
    },

    async resetPassword(id: string, newPassword: string): Promise<boolean> {
      const response = await http.post<boolean>(`/api/user/${id}/reset-password`, {
        newPassword,
      });
      return response.data;
    },

    async lock(id: string, lockoutEnd: string): Promise<boolean> {
      const response = await http.post<boolean>(`/api/user/${id}/lock`, {
        lockoutEnd,
      });
      return response.data;
    },

    async unlock(id: string): Promise<boolean> {
      const response = await http.post<boolean>(`/api/user/${id}/unlock`);
      return response.data;
    },
  },
};
