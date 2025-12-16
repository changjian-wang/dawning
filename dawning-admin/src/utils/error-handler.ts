/**
 * 统一错误处理工具
 */
import { Message, Notification } from '@arco-design/web-vue';
import type { AxiosError } from 'axios';

// 错误级别
export type ErrorLevel = 'info' | 'warning' | 'error' | 'success';

// 错误配置
export interface ErrorConfig {
  /** 是否显示消息 */
  showMessage?: boolean;
  /** 是否显示通知 */
  showNotification?: boolean;
  /** 消息持续时间 (ms) */
  duration?: number;
  /** 自定义错误消息 */
  customMessage?: string;
  /** 错误级别 */
  level?: ErrorLevel;
}

// 默认配置
const DEFAULT_CONFIG: ErrorConfig = {
  showMessage: true,
  showNotification: false,
  duration: 5000,
  level: 'error',
};

/**
 * 从错误对象中提取消息
 */
export function extractErrorMessage(error: unknown): string {
  if (!error) return '未知错误';

  // Axios 错误
  if (isAxiosError(error)) {
    const axiosError = error as AxiosError<{ message?: string; msg?: string }>;

    // 后端返回的错误消息
    if (axiosError.response?.data?.message) {
      return axiosError.response.data.message;
    }
    if (axiosError.response?.data?.msg) {
      return axiosError.response.data.msg;
    }

    // HTTP 状态错误
    if (axiosError.response?.status) {
      return getHttpStatusMessage(axiosError.response.status);
    }

    // 网络错误
    if (axiosError.code) {
      return getNetworkErrorMessage(axiosError.code);
    }

    return axiosError.message || '请求失败';
  }

  // 普通 Error 对象
  if (error instanceof Error) {
    return error.message;
  }

  // 字符串错误
  if (typeof error === 'string') {
    return error;
  }

  // 对象错误
  if (typeof error === 'object' && error !== null) {
    const errorObj = error as Record<string, unknown>;
    if (typeof errorObj.message === 'string') {
      return errorObj.message;
    }
    if (typeof errorObj.msg === 'string') {
      return errorObj.msg;
    }
  }

  return '操作失败，请稍后重试';
}

/**
 * 判断是否是 Axios 错误
 */
function isAxiosError(error: unknown): boolean {
  return (
    typeof error === 'object' &&
    error !== null &&
    'isAxiosError' in error &&
    (error as { isAxiosError: boolean }).isAxiosError === true
  );
}

/**
 * 获取 HTTP 状态码对应的消息
 */
function getHttpStatusMessage(status: number): string {
  const messages: Record<number, string> = {
    400: '请求参数错误',
    401: '未授权，请重新登录',
    403: '拒绝访问，权限不足',
    404: '请求的资源不存在',
    405: '请求方法不允许',
    408: '请求超时',
    409: '资源冲突',
    422: '请求数据验证失败',
    429: '请求过于频繁，请稍后再试',
    500: '服务器内部错误',
    502: '网关错误',
    503: '服务不可用',
    504: '网关超时',
  };

  return messages[status] || `请求失败 (${status})`;
}

/**
 * 获取网络错误对应的消息
 */
function getNetworkErrorMessage(code: string): string {
  const messages: Record<string, string> = {
    ECONNABORTED: '请求超时，请检查网络连接',
    ENOTFOUND: '无法连接到服务器',
    ECONNREFUSED: '服务器拒绝连接',
    ECONNRESET: '连接被重置',
    ERR_NETWORK: '网络连接失败，请检查网络',
    ERR_CANCELED: '请求已取消',
  };

  return messages[code] || '网络错误';
}

/**
 * 统一处理错误
 */
export function handleError(error: unknown, config: ErrorConfig = {}): string {
  const finalConfig = { ...DEFAULT_CONFIG, ...config };
  const message = finalConfig.customMessage || extractErrorMessage(error);

  if (finalConfig.showMessage) {
    switch (finalConfig.level) {
      case 'info':
        Message.info({ content: message, duration: finalConfig.duration });
        break;
      case 'warning':
        Message.warning({ content: message, duration: finalConfig.duration });
        break;
      case 'success':
        Message.success({ content: message, duration: finalConfig.duration });
        break;
      case 'error':
      default:
        Message.error({ content: message, duration: finalConfig.duration });
        break;
    }
  }

  if (finalConfig.showNotification) {
    Notification[finalConfig.level || 'error']({
      title: finalConfig.level === 'error' ? '错误' : '提示',
      content: message,
      duration: finalConfig.duration,
    });
  }

  // 记录到控制台
  console.error('[Error Handler]', error);

  return message;
}

/**
 * 包装异步函数，自动处理错误
 */
export function withErrorHandler<T extends unknown[], R>(
  fn: (...args: T) => Promise<R>,
  config: ErrorConfig = {}
): (...args: T) => Promise<R | undefined> {
  return async (...args: T): Promise<R | undefined> => {
    try {
      return await fn(...args);
    } catch (error) {
      handleError(error, config);
      return undefined;
    }
  };
}

/**
 * 用于 try-catch 的简化版错误处理
 * @example
 * try {
 *   await someAsyncOperation();
 * } catch (error) {
 *   showError(error);
 * }
 */
export function showError(error: unknown, customMessage?: string): void {
  handleError(error, { customMessage });
}

/**
 * 显示成功消息
 */
export function showSuccess(message: string, duration = 3000): void {
  Message.success({ content: message, duration });
}

/**
 * 显示警告消息
 */
export function showWarning(message: string, duration = 5000): void {
  Message.warning({ content: message, duration });
}

/**
 * 显示信息消息
 */
export function showInfo(message: string, duration = 3000): void {
  Message.info({ content: message, duration });
}

export default {
  extractErrorMessage,
  handleError,
  withErrorHandler,
  showError,
  showSuccess,
  showWarning,
  showInfo,
};
