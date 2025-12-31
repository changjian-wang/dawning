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
    // Check if user has specified permission
    hasPermission: (state) => (permission: string) => {
      // Super admin has all permissions
      if (state.role === 'super_admin' || state.roles.includes('super_admin')) {
        return true;
      }
      return state.permissions.includes(permission);
    },
    // Check if user has any of the specified permissions
    hasAnyPermission: (state) => (permissions: string[]) => {
      if (state.role === 'super_admin' || state.roles.includes('super_admin')) {
        return true;
      }
      return permissions.some((p) => state.permissions.includes(p));
    },
    // Check if user has all specified permissions
    hasAllPermissions: (state) => (permissions: string[]) => {
      if (state.role === 'super_admin' || state.roles.includes('super_admin')) {
        return true;
      }
      return permissions.every((p) => state.permissions.includes(p));
    },
  },

  actions: {
    // Switch between user's multiple roles
    switchRoles() {
      return new Promise((resolve) => {
        if (this.roles.length <= 1) {
          // Only one role, cannot switch
          resolve(this.role);
          return;
        }
        // Find current role index and switch to next
        const currentIndex = this.roles.indexOf(this.role);
        const nextIndex = (currentIndex + 1) % this.roles.length;
        this.role = this.roles[nextIndex] as RoleType;
        // Refresh permissions after switching roles
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
      const userData = res.data as any;
      
      // Handle roles (may be a single string or array)
      let roles: string[] = [];
      if (Array.isArray(userData.roles)) {
        roles = userData.roles;
      } else if (userData.roles) {
        roles = [userData.roles];
      }
      const primaryRole = (roles[0] || 'user') as RoleType;
      
      this.setInfo({
        name: userData.name || userData.username,
        email: userData.email,
        avatar: userData.avatar,
        role: primaryRole,
        roles,
        accountId: userData.id,
      });
      
      // Get user permissions
      await this.fetchPermissions();
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

        // Store tokens
        setToken(access_token);
        if (refresh_token) {
          setRefreshToken(refresh_token);
        }
        if (id_token) {
          setIdToken(id_token);
        }
        setTokenExpiresAt(expires_in);

        // Note: Role info only exists in access_token, id_token does not contain roles
        // Get role info from access_token
        const accessTokenInfo = parseJwtToken(access_token);
        // Get user basic info from id_token (if available)
        const idTokenInfo = id_token ? parseJwtToken(id_token) : null;

        if (accessTokenInfo) {
          // Handle roles (may be a single string or array) - get from access_token
          let roles: string[] = [];
          if (Array.isArray(accessTokenInfo.role)) {
            roles = accessTokenInfo.role;
          } else if (accessTokenInfo.role) {
            roles = [accessTokenInfo.role];
          }
          const primaryRole = (roles[0] || 'user') as RoleType;

          // Prefer user info from id_token, get role info from access_token
          const userInfo = idTokenInfo || accessTokenInfo;

          this.setInfo({
            name: userInfo.name || userInfo.sub,
            email: userInfo.email,
            role: primaryRole,
            roles,
            accountId: userInfo.sub,
          });

          // Get user permissions after login
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
