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

// Error code constants
const ERROR_CODES = {
  SUCCESS: 20000,
  ILLEGAL_TOKEN: 50008,
  OTHER_CLIENT_LOGIN: 50012,
  TOKEN_EXPIRED: 50014,
} as const;

// HTTP status code error message mapping
const HTTP_ERROR_MESSAGES: Record<number, string> = {
  400: 'Invalid request parameters',
  401: 'Unauthorized, please login again',
  403: 'Access denied, insufficient permissions',
  404: 'The requested resource does not exist',
  405: 'Request method not allowed',
  408: 'Request timeout',
  409: 'Resource conflict',
  410: 'Resource has been deleted',
  422: 'Request data validation failed',
  429: 'Too many requests, please try again later',
  500: 'Internal server error',
  501: 'Service not implemented',
  502: 'Gateway error',
  503: 'Service unavailable',
  504: 'Gateway timeout',
};

// Network error messages
const NETWORK_ERROR_MESSAGES: Record<string, string> = {
  ECONNABORTED: 'Request timeout, please check your network connection',
  ENOTFOUND: 'Cannot connect to server',
  ECONNREFUSED: 'Server refused connection',
  ECONNRESET: 'Connection reset',
  ERR_NETWORK: 'Network connection failed, please check your network',
  ERR_CANCELED: 'Request cancelled',
};

// Error codes that require auto logout
const LOGOUT_ERROR_CODES: number[] = [
  ERROR_CODES.ILLEGAL_TOKEN,
  ERROR_CODES.OTHER_CLIENT_LOGIN,
  ERROR_CODES.TOKEN_EXPIRED,
];

// Retry configuration
const RETRY_CONFIG = {
  maxRetries: 3,
  retryDelay: 1000,
  retryStatusCodes: [408, 429, 500, 502, 503, 504],
};

// Get user-friendly error message
function getErrorMessage(error: AxiosError<HttpResponse>): string {
  // 1. Check if there is a business error message from backend
  if (error.response?.data) {
    const { message, msg } = error.response.data;
    if (message && message !== 'Error') return message;
    if (msg && msg !== 'Error') return msg;
  }

  // 2. Check HTTP status code
  if (error.response?.status) {
    const httpMessage = HTTP_ERROR_MESSAGES[error.response.status];
    if (httpMessage) return httpMessage;
  }

  // 3. Check network error
  if (error.code) {
    const networkMessage = NETWORK_ERROR_MESSAGES[error.code];
    if (networkMessage) return networkMessage;
  }

  // 4. Check if it is a timeout
  if (error.message?.includes('timeout')) {
    return 'Request timeout, please try again later';
  }

  // 5. Check if it is a network error
  if (error.message?.includes('Network Error')) {
    return 'Network connection failed, please check your network';
  }

  // 6. Default error message
  return error.message || 'Request failed, please try again later';
}

// Delay function
const delay = (ms: number) =>
  new Promise<void>((resolve) => {
    setTimeout(resolve, ms);
  });

// Determine if request should be retried
function shouldRetry(error: AxiosError, retryCount: number): boolean {
  // Exceeded max retry count
  if (retryCount >= RETRY_CONFIG.maxRetries) return false;

  // Do not retry cancelled requests
  if (axios.isCancel(error)) return false;

  // Network errors can be retried
  if (!error.response) return true;

  // Specific status codes can be retried
  return RETRY_CONFIG.retryStatusCodes.includes(error.response.status);
}

// Set base URL (empty string means using relative path)
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
if (apiBaseUrl && apiBaseUrl.trim() !== '') {
  axios.defaults.baseURL = apiBaseUrl;
}

// Token refresh related
let isRefreshing = false;
let refreshSubscribers: ((token: string) => void)[] = [];

// Add subscriber waiting for token refresh
function subscribeTokenRefresh(callback: (token: string) => void) {
  refreshSubscribers.push(callback);
}

// Notify all subscribers that token has been refreshed
function onTokenRefreshed(token: string) {
  refreshSubscribers.forEach((callback) => callback(token));
  refreshSubscribers = [];
}

// Request interceptor
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

// Handle auto logout - clear token and refresh page to avoid circular dependency
const handleAutoLogout = () => {
  clearToken();
  window.location.href = '/login';
};

// Show logout confirmation modal
const showLogoutModal = () => {
  Modal.error({
    title: 'Confirm logout',
    content:
      'You have been logged out, you can cancel to stay on this page, or log in again',
    okText: 'Re-Login',
    onOk: handleAutoLogout,
  });
};

// Response interceptor
axios.interceptors.response.use(
  (response: AxiosResponse<HttpResponse>) => {
    const { data } = response;

    // OAuth endpoint returns raw response directly
    if (response.config.url?.includes('/connect/token')) {
      return response;
    }

    // If response data does not conform to standard format (no code field), return raw response
    if (!data || typeof data !== 'object' || !('code' in data)) {
      return response;
    }

    // Check business status code
    if (data.code !== ERROR_CODES.SUCCESS) {
      // Show error message
      Message.error({
        content: data.message || data.msg || 'Error',
        duration: 5000,
      });

      // Handle error codes that require logout
      const shouldLogout =
        LOGOUT_ERROR_CODES.includes(data.code) &&
        response.config?.url !== '/api/user/info';

      if (shouldLogout) {
        showLogoutModal();
      }

      return Promise.reject(new Error(data.message || data.msg || 'Error'));
    }

    // Return business data - backend returns { code: 0, message: "Success", data: {...} }
    // We directly return the content of the data field
    return { ...response, data: data.data !== undefined ? data.data : data };
  },
  async (error: AxiosError<HttpResponse>) => {
    // Handle 401 unauthorized error - try to refresh token
    if (
      error.response?.status === 401 &&
      error.config?.url !== '/connect/token'
    ) {
      const originalRequest = error.config;
      const refreshToken = getRefreshToken();

      // If there is a refresh token and not in refresh process, try to refresh
      if (refreshToken && !isRefreshing) {
        isRefreshing = true;

        try {
          const response = await refreshAccessToken(refreshToken);
          const { access_token, refresh_token, id_token, expires_in } =
            response.data;

          // Update tokens
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

          // Notify all waiting requests
          onTokenRefreshed(access_token);
          isRefreshing = false;

          // Retry original request
          if (originalRequest?.headers) {
            originalRequest.headers.Authorization = `Bearer ${access_token}`;
          }
          if (originalRequest) {
            return axios(originalRequest);
          }
          return Promise.reject(new Error('No original request'));
        } catch (refreshError) {
          // Refresh failed, redirect to login
          isRefreshing = false;
          refreshSubscribers = [];
          showLogoutModal();
          return Promise.reject(refreshError);
        }
      }

      // If refreshing, add request to queue
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

      // No refresh token, logout directly
      showLogoutModal();
      return Promise.reject(error);
    }

    // Get current retry count
    const config = error.config as AxiosRequestConfig & { retryCount?: number };
    const retryCount = config?.retryCount || 0;

    // Determine if should auto retry
    if (shouldRetry(error, retryCount)) {
      config.retryCount = retryCount + 1;

      // Calculate delay time (exponential backoff)
      const delayMs = RETRY_CONFIG.retryDelay * 2 ** retryCount;

      // eslint-disable-next-line no-console
      console.log(
        `[HTTP] Request failed, retrying attempt ${config.retryCount} after ${delayMs}ms...`,
        error.config?.url
      );

      await delay(delayMs);
      return axios(config);
    }

    // Get user-friendly error message
    const message = getErrorMessage(error);

    // Show error message
    Message.error({
      content: message,
      duration: 5000,
    });

    return Promise.reject(error);
  }
);

// Export configured axios instance
export default axios;
