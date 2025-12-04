import axios from 'axios';
import type { AxiosRequestConfig, AxiosResponse, AxiosError } from 'axios';
import { Message, Modal } from '@arco-design/web-vue';
import { useUserStore } from '@/store';
import { getToken, getRefreshToken, isLogin } from '@/utils/auth';
import { refreshAccessToken } from './auth';

export interface HttpResponse<T = unknown> {
  status: number;
  msg: string;
  code: number;
  data: T;
}

// 错误码常量
const ERROR_CODES = {
  SUCCESS: 20000,
  ILLEGAL_TOKEN: 50008,
  OTHER_CLIENT_LOGIN: 50012,
  TOKEN_EXPIRED: 50014,
} as const;

// 需要自动登出的错误码
const LOGOUT_ERROR_CODES: number[] = [
  ERROR_CODES.ILLEGAL_TOKEN,
  ERROR_CODES.OTHER_CLIENT_LOGIN,
  ERROR_CODES.TOKEN_EXPIRED,
];

// 设置基础URL
if (import.meta.env.VITE_API_BASE_URL) {
  axios.defaults.baseURL = import.meta.env.VITE_API_BASE_URL;
}

// Token 刷新相关
let isRefreshing = false;
let refreshSubscribers: ((token: string) => void)[] = [];

// 添加订阅者等待 token 刷新
function subscribeTokenRefresh(callback: (token: string) => void) {
  refreshSubscribers.push(callback);
}

// 通知所有订阅者 token 已刷新
function onTokenRefreshed(token: string) {
  refreshSubscribers.forEach((callback) => callback(token));
  refreshSubscribers = [];
}

// 请求拦截器
axios.interceptors.request.use(
  (config: AxiosRequestConfig) => {
    const token = getToken();
    if (token && config.url !== '/connect/token') {
      config.headers = {
        ...config.headers,
        Authorization: `Bearer ${token}`,
      };
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// 处理自动登出
const handleAutoLogout = async () => {
  try {
    const userStore = useUserStore();
    await userStore.logout();
    window.location.reload();
  } catch (error) {
    console.error('Logout failed:', error);
  }
};

// 显示登出确认弹窗
const showLogoutModal = () => {
  Modal.error({
    title: 'Confirm logout',
    content:
      'You have been logged out, you can cancel to stay on this page, or log in again',
    okText: 'Re-Login',
    onOk: handleAutoLogout,
  });
};

// 响应拦截器
axios.interceptors.response.use(
  (response: AxiosResponse<HttpResponse>) => {
    const { data } = response;

    // OAuth 端点直接返回原始响应
    if (response.config.url?.includes('/connect/token')) {
      return response;
    }

    // 检查业务状态码
    if (data.code !== ERROR_CODES.SUCCESS) {
      // 显示错误消息
      Message.error({
        content: data.msg || 'Error',
        duration: 5000,
      });

      // 处理需要登出的错误码
      const shouldLogout =
        LOGOUT_ERROR_CODES.includes(data.code) &&
        response.config.url !== '/api/user/info';

      if (shouldLogout) {
        showLogoutModal();
      }

      return Promise.reject(new Error(data.msg || 'Error'));
    }

    return data; // 直接返回 data，而不是整个 response
  },
  async (error: AxiosError<HttpResponse>) => {
    // 处理 401 未授权错误 - 尝试刷新 token
    if (
      error.response?.status === 401 &&
      error.config?.url !== '/connect/token'
    ) {
      const originalRequest = error.config;
      const refreshToken = getRefreshToken();

      // 如果有 refresh token 且不在刷新过程中，尝试刷新
      if (refreshToken && !isRefreshing) {
        isRefreshing = true;

        try {
          const response = await refreshAccessToken(refreshToken);
          const { access_token, refresh_token, id_token, expires_in } =
            response.data;

          // 更新 tokens
          const { setToken, setRefreshToken, setIdToken, setTokenExpiresAt } =
            await import('@/utils/auth');
          setToken(access_token);
          if (refresh_token) {
            setRefreshToken(refresh_token);
          }
          if (id_token) {
            setIdToken(id_token);
          }
          setTokenExpiresAt(expires_in);

          // 通知所有等待的请求
          onTokenRefreshed(access_token);
          isRefreshing = false;

          // 重试原始请求
          if (originalRequest?.headers) {
            originalRequest.headers.Authorization = `Bearer ${access_token}`;
          }
          return axios(originalRequest!);
        } catch (refreshError) {
          // 刷新失败，跳转登录
          isRefreshing = false;
          refreshSubscribers = [];
          showLogoutModal();
          return Promise.reject(refreshError);
        }
      }

      // 如果正在刷新，将请求加入队列
      if (isRefreshing) {
        return new Promise((resolve) => {
          subscribeTokenRefresh((token: string) => {
            if (originalRequest?.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            resolve(axios(originalRequest!));
          });
        });
      }

      // 没有 refresh token，直接登出
      showLogoutModal();
      return Promise.reject(error);
    }

    // 处理网络错误或其他HTTP错误
    const message =
      error.response?.data?.msg || error.message || 'Request Error';

    Message.error({
      content: message,
      duration: 5000,
    });

    return Promise.reject(error);
  }
);

// 导出配置好的 axios 实例
export default axios;

// 导出一些常用的请求方法
export const http = {
  get: <T = any>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<HttpResponse<T>> => axios.get(url, config),

  post: <T = any>(
    url: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<HttpResponse<T>> => axios.post(url, data, config),

  put: <T = any>(
    url: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<HttpResponse<T>> => axios.put(url, data, config),

  delete: <T = any>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<HttpResponse<T>> => axios.delete(url, config),
};
