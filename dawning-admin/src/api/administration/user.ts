import { http, isSuccessCode } from '../interceptor';
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
  username?: string; // 用于显示，不可修改
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
      username: source.username, // 用于显示，不会发送到服务器
      email: source.email,
      phoneNumber: source.phoneNumber,
      displayName: source.displayName,
      avatar: source.avatar,
      role: source.role,
      isActive: source.isActive,
      remark: source.remark,
    }),

    isValid: (form: ICreateUserModel | IUpdateUserModel): boolean => {
      if ('password' in form) {
        // 创建用户时需要验证用户名和密码
        return !!(form.username?.trim() && form.password?.trim());
      }
      // 更新用户时不需要验证
      return true;
    },
  },

  api: {
    // 获取用户详情
    async get(id: string): Promise<IUser> {
      const response = await http.get(`/api/user/${id}`);
      return response.data;
    },

    // 获取用户列表（分页）
    async getPagedList(
      model: IUserModel,
      page: number,
      pageSize: number
    ): Promise<IPagedData<IUser>> {
      const response = await http.get(`/api/user`, {
        params: {
          page,
          pageSize,
          ...model,
        },
      });
      // 拦截器返回 { code, message, data }，response.data 包含 { list, pagination }
      const { list, pagination } = response.data;
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
      return response.data;
    },

    // 更新用户
    async update(model: IUpdateUserModel): Promise<IUser> {
      const response = await http.put(`/api/user/${model.id}`, model);
      return response.data;
    },

    // 删除用户
    async delete(id: string): Promise<boolean> {
      const response = await http.delete(`/api/user/${id}`);
      return isSuccessCode(response.code);
    },

    // 重置密码
    async resetPassword(id: string, newPassword: string): Promise<boolean> {
      const response = await http.post(`/api/user/${id}/reset-password`, {
        newPassword,
      });
      return isSuccessCode(response.code);
    },

    // 修改密码
    async changePassword(
      userId: string,
      oldPassword: string,
      newPassword: string
    ): Promise<boolean> {
      const response = await http.post('/api/user/change-password', {
        userId,
        oldPassword,
        newPassword,
      });
      return isSuccessCode(response.code);
    },
  },
};
