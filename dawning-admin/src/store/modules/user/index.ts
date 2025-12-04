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
import { UserState } from './types';
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
  }),

  getters: {
    userInfo(state: UserState): UserState {
      return { ...state };
    },
  },

  actions: {
    switchRoles() {
      return new Promise((resolve) => {
        this.role = this.role === 'user' ? 'admin' : 'user';
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

        // 从 ID Token 或 Access Token 中解析用户信息
        const userInfo = parseJwtToken(id_token || access_token);
        if (userInfo) {
          this.setInfo({
            name: userInfo.name || userInfo.sub,
            email: userInfo.email,
            role: userInfo.role || 'admin',
            accountId: userInfo.sub,
          });
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
