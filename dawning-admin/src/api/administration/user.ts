import { http } from '../interceptor';
import { IPagedData } from '../paged-data';

// 用户DTO（匹配后端UserDto）
export interface IUser {
  id: string;
  username: string;
  email?: string;
  phoneNumber?: string;
  displayName?: string;
  avatar?: string;
  role: string;
  isActive: boolean;
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt?: string;
  remark?: string;
  timestamp: number;
}

// 查询模型
export interface IUserModel {
  username?: string;
  email?: string;
  displayName?: string;
  role?: string;
  isActive?: boolean;
}

// 创建用户请求
export interface ICreateUserModel {
  username: string;
  password: string;
  email?: string;
  phoneNumber?: string;
  displayName?: string;
  avatar?: string;
  role?: string;
  isActive?: boolean;
  remark?: string;
}

// 更新用户请求
export interface IUpdateUserModel {
  id: string;
  email?: string;
  phoneNumber?: string;
  displayName?: string;
  avatar?: string;
  role?: string;
  isActive?: boolean;
  remark?: string;
}

// 重置密码请求
export interface IResetPasswordModel {
  newPassword: string;
}

// 统一的 user 对象
export const user = {
  // 表单工厂
  form: {
    create: (): ICreateUserModel => ({
      username: '',
      password: '',
      email: '',
      phoneNumber: '',
      displayName: '',
      avatar: '',
      role: 'user',
      isActive: true,
      remark: '',
    }),

    reset: (target: ICreateUserModel) => {
      Object.assign(target, user.form.create());
    },

    clone: (source: IUser): IUpdateUserModel => ({
      id: source.id,
      email: source.email,
      phoneNumber: source.phoneNumber,
      displayName: source.displayName,
      avatar: source.avatar,
      role: source.role,
      isActive: source.isActive,
      remark: source.remark,
    }),

    isValid: (form: ICreateUserModel | IUpdateUserModel): boolean => {
      if ('username' in form) {
        return !!(
          form.username?.trim() &&
          form.password?.trim()
        );
      }
      return true;
    },
  },
  
  api: {
    // 获取用户详情
    async get(id: string): Promise<IUser> {
      const response = await http.get(`/api/user/${id}`);
      return response.data.data;
    },

    // 获取用户列表（分页）
    async getPagedList(
      model: IUserModel,
      page: number,
      pageSize: number
    ): Promise<IPagedData<IUser>> {
      const response = await http.get(
        `/api/user`,
        {
          params: {
            page,
            pageSize,
            ...model,
          },
        }
      );
      // 后端返回格式：{ code, message, data: { list, pagination } }
      const { list, pagination } = response.data.data;
      return {
        items: list,
        totalCount: pagination.total,
        pageIndex: pagination.current,
        pageSize: pagination.pageSize,
      };
    },

    // 创建用户
    async create(model: ICreateUserModel): Promise<IUser> {
      const response = await http.post('/api/user', model);
      return response.data.data;
    },

    // 更新用户
    async update(model: IUpdateUserModel): Promise<IUser> {
      const response = await http.put(`/api/user/${model.id}`, model);
      return response.data.data;
    },

    // 删除用户
    async delete(id: string): Promise<boolean> {
      const response = await http.delete(`/api/user/${id}`);
      return response.data.code === 0;
    },

    // 重置密码
    async resetPassword(id: string, newPassword: string): Promise<boolean> {
      const response = await http.post(`/api/user/${id}/reset-password`, {
        newPassword,
      });
      return response.data.code === 0;
    },

    // 修改密码
    async changePassword(userId: string, oldPassword: string, newPassword: string): Promise<boolean> {
      const response = await http.post('/api/user/change-password', {
        userId,
        oldPassword,
        newPassword,
      });
      return response.data.code === 0;
    },
  },
};
