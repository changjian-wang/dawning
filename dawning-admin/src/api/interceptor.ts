import axios from 'axios';
import type { AxiosRequestConfig, AxiosResponse, AxiosError } from 'axios';
import { Message, Modal } from '@arco-design/web-vue';
import { getToken, getRefreshToken, clearToken } from '@/utils/auth';
import { refreshAccessToken } from './auth';

export interface HttpResponse<T = unknown> {
  code: number;
  message?: string;
  msg?: string;
  data: T;
}

// 错误码常量
const ERROR_CODES = {
  SUCCESS: 20000,
  ILLEGAL_TOKEN: 50008,
  OTHER_CLIENT_LOGIN: 50012,
  TOKEN_EXPIRED: 50014,
} as const;

// HTTP 状态码错误消息映射
const HTTP_ERROR_MESSAGES: Record<number, string> = {
  400: '请求参数错误',
  401: '未授权，请重新登录',
  403: '拒绝访问，权限不足',
  404: '请求的资源不存在',
  405: '请求方法不允许',
  408: '请求超时',
  409: '资源冲突',
  410: '资源已被删除',
  422: '请求数据验证失败',
  429: '请求过于频繁，请稍后再试',
  500: '服务器内部错误',
  501: '服务未实现',
  502: '网关错误',
  503: '服务不可用',
  504: '网关超时',
};

// 网络错误消息
const NETWORK_ERROR_MESSAGES: Record<string, string> = {
  ECONNABORTED: '请求超时，请检查网络连接',
  ENOTFOUND: '无法连接到服务器',
  ECONNREFUSED: '服务器拒绝连接',
  ECONNRESET: '连接被重置',
  ERR_NETWORK: '网络连接失败，请检查网络',
  ERR_CANCELED: '请求已取消',
};

// 需要自动登出的错误码
const LOGOUT_ERROR_CODES: number[] = [
  ERROR_CODES.ILLEGAL_TOKEN,
  ERROR_CODES.OTHER_CLIENT_LOGIN,
  ERROR_CODES.TOKEN_EXPIRED,
];

// 重试配置
const RETRY_CONFIG = {
  maxRetries: 3,
  retryDelay: 1000,
  retryStatusCodes: [408, 429, 500, 502, 503, 504],
};

// 获取友好的错误消息
function getErrorMessage(error: AxiosError<HttpResponse>): string {
  // 1. 检查是否有后端返回的业务错误消息
  if (error.response?.data) {
    const { message, msg } = error.response.data;
    if (message && message !== 'Error') return message;
    if (msg && msg !== 'Error') return msg;
  }

  // 2. 检查 HTTP 状态码
  if (error.response?.status) {
    const httpMessage = HTTP_ERROR_MESSAGES[error.response.status];
    if (httpMessage) return httpMessage;
  }

  // 3. 检查网络错误
  if (error.code) {
    const networkMessage = NETWORK_ERROR_MESSAGES[error.code];
    if (networkMessage) return networkMessage;
  }

  // 4. 检查是否是超时
  if (error.message?.includes('timeout')) {
    return '请求超时，请稍后重试';
  }

  // 5. 检查是否是网络错误
  if (error.message?.includes('Network Error')) {
    return '网络连接失败，请检查网络';
  }

  // 6. 默认错误消息
  return error.message || '请求失败，请稍后重试';
}

// 延迟函数
const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

// 判断是否应该重试
function shouldRetry(error: AxiosError, retryCount: number): boolean {
  // 超过最大重试次数
  if (retryCount >= RETRY_CONFIG.maxRetries) return false;

  // 请求被取消不重试
  if (axios.isCancel(error)) return false;

  // 网络错误可以重试
  if (!error.response) return true;

  // 特定状态码可以重试
  return RETRY_CONFIG.retryStatusCodes.includes(error.response.status);
}

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

// 处理自动登出 - 直接清除 token 并刷新页面，避免循环依赖
const handleAutoLogout = () => {
  clearToken();
  window.location.href = '/login';
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

    // 如果响应数据不符合标准格式（没有 code 字段），直接返回原始响应
    if (!data || typeof data !== 'object' || !('code' in data)) {
      return response;
    }

    // 检查业务状态码
    if (data.code !== ERROR_CODES.SUCCESS) {
      // 显示错误消息
      Message.error({
        content: data.message || data.msg || 'Error',
        duration: 5000,
      });

      // 处理需要登出的错误码
      const shouldLogout =
        LOGOUT_ERROR_CODES.includes(data.code) &&
        response.config?.url !== '/api/user/info';

      if (shouldLogout) {
        showLogoutModal();
      }

      return Promise.reject(new Error(data.message || data.msg || 'Error'));
    }

    // 返回业务数据 - 后端返回 { code: 0, message: "Success", data: {...} }
    // 我们直接返回 data 字段的内容
    return { ...response, data: data.data !== undefined ? data.data : data };
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
          if (originalRequest) {
            return axios(originalRequest);
          }
          return Promise.reject(new Error('No original request'));
        } catch (refreshError) {
          // 刷新失败，跳转登录
          isRefreshing = false;
          refreshSubscribers = [];
          showLogoutModal();
          return Promise.reject(refreshError);
        }
      }

      // 如果正在刷新,将请求加入队列
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          subscribeTokenRefresh((token: string) => {
            if (originalRequest?.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            if (originalRequest) {
              resolve(axios(originalRequest));
            } else {
              reject(new Error('No original request'));
            }
          });
        });
      }

      // 没有 refresh token，直接登出
      showLogoutModal();
      return Promise.reject(error);
    }

    // 获取当前重试次数
    const config = error.config as AxiosRequestConfig & { _retryCount?: number };
    const retryCount = config?._retryCount || 0;

    // 判断是否应该自动重试
    if (shouldRetry(error, retryCount)) {
      config._retryCount = retryCount + 1;

      // 计算延迟时间（指数退避）
      const delayMs = RETRY_CONFIG.retryDelay * Math.pow(2, retryCount);

      console.log(
        `[HTTP] 请求失败，${delayMs}ms 后进行第 ${config._retryCount} 次重试...`,
        error.config?.url
      );

      await delay(delayMs);
      return axios(config);
    }

    // 获取友好的错误消息
    const message = getErrorMessage(error);

    // 显示错误消息
    Message.error({
      content: message,
      duration: 5000,
    });

    return Promise.reject(error);
  }
);

// 导出配置好的 axios 实例
export default axios;
