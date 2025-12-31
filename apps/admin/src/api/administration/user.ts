import axios from '@/api/interceptor';
import { IPagedData } from '../paged-data';

// User DTO (matches backend UserDto)
export interface IUser {
  id: string;
  username: string;
  email?: string;
  phoneNumber?: string;
  displayName?: string;
  avatar?: string;
  role: string;
  isActive: boolean;
  isSystem: boolean;
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt?: string;
  remark?: string;
  timestamp: number;
}

// Query model
export interface IUserModel {
  username?: string;
  email?: string;
  displayName?: string;
  role?: string;
  isActive?: boolean;
}

// Create user request
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

// Update user request
export interface IUpdateUserModel {
  id: string;
  username?: string; // For display, not editable
  email?: string;
  phoneNumber?: string;
  displayName?: string;
  avatar?: string;
  role?: string;
  isActive?: boolean;
  remark?: string;
}

// Reset password request
export interface IResetPasswordModel {
  newPassword: string;
}

// Unified user object
export const user = {
  // Form factory
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
      username: source.username, // For display, not sent to server
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
        // When creating user, validate username and password
        return !!(form.username?.trim() && form.password?.trim());
      }
      // No validation needed when updating user
      return true;
    },
  },

  api: {
    // Get user details
    async get(id: string): Promise<IUser> {
      const response = await axios.get<IUser>(`/api/user/${id}`);
      return response.data;
    },

    // Get user list (paginated)
    async getPagedList(
      model: IUserModel,
      page: number,
      pageSize: number
    ): Promise<IPagedData<IUser>> {
      const response = await axios.get<{
        list: IUser[];
        pagination: { total: number; current: number; pageSize: number };
      }>(`/api/user`, {
        params: {
          page,
          pageSize,
          ...model,
        },
      });

      // response.data is now the business data directly {list, pagination}
      const { list, pagination } = response.data;

      return {
        items: list,
        totalCount: pagination.total,
        pageIndex: pagination.current,
        pageSize: pagination.pageSize,
      };
    },

    // Create user
    async create(model: ICreateUserModel): Promise<IUser> {
      const response = await axios.post<IUser>('/api/user', model);
      return response.data;
    },

    // Update user
    async update(model: IUpdateUserModel): Promise<IUser> {
      const response = await axios.put<IUser>(`/api/user/${model.id}`, model);
      return response.data;
    },

    // Delete user
    async delete(id: string): Promise<boolean> {
      await axios.delete(`/api/user/${id}`);
      return true;
    },

    // Reset password
    async resetPassword(id: string, newPassword: string): Promise<boolean> {
      await axios.post(`/api/user/${id}/reset-password`, {
        newPassword,
      });
      return true;
    },

    // Change password
    async changePassword(
      userId: string,
      oldPassword: string,
      newPassword: string
    ): Promise<boolean> {
      await axios.post('/api/user/change-password', {
        userId,
        oldPassword,
        newPassword,
      });
      return true;
    },

    // Get user roles list
    async getUserRoles(userId: string): Promise<any[]> {
      const response = await axios.get(`/api/user/${userId}/roles`);
      return response.data;
    },

    // Assign roles to user
    async assignRoles(userId: string, roleIds: string[]): Promise<boolean> {
      await axios.post(`/api/user/${userId}/roles`, { roleIds });
      return true;
    },

    // Batch delete users
    async batchDelete(ids: string[]): Promise<{
      successCount: number;
      failedCount: number;
      failedIds: string[];
    }> {
      const response = await axios.delete('/api/user/batch', { data: { ids } });
      return response.data;
    },

    // Batch update user status
    async batchUpdateStatus(
      ids: string[],
      isActive: boolean
    ): Promise<{
      successCount: number;
      failedCount: number;
      failedIds: string[];
    }> {
      const response = await axios.post('/api/user/batch/status', {
        ids,
        isActive,
      });
      return response.data;
    },
  },
};
