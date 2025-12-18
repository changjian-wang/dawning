import { defineStore } from 'pinia';
import { logout as userLogout, getUserInfo } from '@/api/user';
import { loginWithPassword, parseJwtToken } from '@/api/auth';
import {
  setToken,
  setRefreshToken,
  setIdToken,
  setTokenExpiresAt,
  clearToken,
} from '@/utils/auth';
import { removeRouteListener } from '@/utils/route-listener';
import { UserState, RoleType } from './types';
import useAppStore from '../app';

export interface LoginData {
  username: string;
  password: string;
}

const useUserStore = defineStore('user', {
  state: (): UserState => ({
    name: undefined,
    avatar: undefined,
    job: undefined,
    organization: undefined,
    location: undefined,
    email: undefined,
    introduction: undefined,
    personalWebsite: undefined,
    jobName: undefined,
    organizationName: undefined,
    locationName: undefined,
    phone: undefined,
    registrationDate: undefined,
    accountId: undefined,
    certification: undefined,
    role: '',
    roles: [],
    permissions: [],
  }),

  getters: {
    userInfo(state: UserState): UserState {
      return { ...state };
    },
    // 检查是否拥有指定权限
    hasPermission: (state) => (permission: string) => {
      // 超级管理员拥有所有权限
      if (state.role === 'super_admin' || state.roles.includes('super_admin')) {
        return true;
      }
      return state.permissions.includes(permission);
    },
    // 检查是否拥有任意一个权限
    hasAnyPermission: (state) => (permissions: string[]) => {
      if (state.role === 'super_admin' || state.roles.includes('super_admin')) {
        return true;
      }
      return permissions.some((p) => state.permissions.includes(p));
    },
    // 检查是否拥有所有权限
    hasAllPermissions: (state) => (permissions: string[]) => {
      if (state.role === 'super_admin' || state.roles.includes('super_admin')) {
        return true;
      }
      return permissions.every((p) => state.permissions.includes(p));
    },
  },

  actions: {
    // 在用户拥有的多个角色之间切换
    switchRoles() {
      return new Promise((resolve) => {
        if (this.roles.length <= 1) {
          // 只有一个角色，无法切换
          resolve(this.role);
          return;
        }
        // 找到当前角色在数组中的索引，切换到下一个
        const currentIndex = this.roles.indexOf(this.role);
        const nextIndex = (currentIndex + 1) % this.roles.length;
        this.role = this.roles[nextIndex] as RoleType;
        // 切换角色后重新获取权限
        this.fetchPermissions();
        resolve(this.role);
      });
    },
    // Set user's information
    setInfo(partial: Partial<UserState>) {
      this.$patch(partial);
    },

    // Reset user's information
    resetInfo() {
      this.$reset();
    },

    // Get user's information
    async info() {
      const res = await getUserInfo();
      this.setInfo(res.data);
    },

    // Fetch user permissions
    async fetchPermissions() {
      if (!this.accountId) return;
      try {
        // Dynamic import to avoid circular dependency
        const { getUserPermissions } = await import(
          '@/api/administration/permission'
        );
        const permissions = await getUserPermissions(this.accountId);
        this.permissions = permissions;
      } catch {
        this.permissions = [];
      }
    },

    // Login with OpenIddict OAuth 2.0
    async login(loginForm: LoginData) {
      try {
        const res = await loginWithPassword(
          loginForm.username,
          loginForm.password
        );
        const { access_token, refresh_token, id_token, expires_in } = res.data;

        // 存储 tokens
        setToken(access_token);
        if (refresh_token) {
          setRefreshToken(refresh_token);
        }
        if (id_token) {
          setIdToken(id_token);
        }
        setTokenExpiresAt(expires_in);

        // 注意：角色信息只存在于 access_token 中，id_token 不包含角色
        // 从 access_token 获取角色信息
        const accessTokenInfo = parseJwtToken(access_token);
        // 从 id_token 获取用户基本信息（如果有的话）
        const idTokenInfo = id_token ? parseJwtToken(id_token) : null;

        if (accessTokenInfo) {
          // 处理角色（可能是单个字符串或数组）- 从 access_token 获取
          let roles: string[] = [];
          if (Array.isArray(accessTokenInfo.role)) {
            roles = accessTokenInfo.role;
          } else if (accessTokenInfo.role) {
            roles = [accessTokenInfo.role];
          }
          const primaryRole = (roles[0] || 'user') as RoleType;

          // 优先使用 id_token 中的用户信息，角色信息从 access_token 获取
          const userInfo = idTokenInfo || accessTokenInfo;

          this.setInfo({
            name: userInfo.name || userInfo.sub,
            email: userInfo.email,
            role: primaryRole,
            roles,
            accountId: userInfo.sub,
          });

          // 登录后获取用户权限
          await this.fetchPermissions();
        }
      } catch (err) {
        clearToken();
        throw err;
      }
    },

    logoutCallBack() {
      const appStore = useAppStore();
      this.resetInfo();
      clearToken();
      removeRouteListener();
      appStore.clearServerMenu();
    },

    // Logout
    async logout() {
      try {
        await userLogout();
      } finally {
        this.logoutCallBack();
      }
    },
  },
});

export default useUserStore;
