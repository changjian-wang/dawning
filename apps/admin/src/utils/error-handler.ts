/**
 * Unified error handling utility
 */
import { Message, Notification } from '@arco-design/web-vue';
import type { AxiosError } from 'axios';

// Error level
export type ErrorLevel = 'info' | 'warning' | 'error' | 'success';

// Error configuration
export interface ErrorConfig {
  /** Whether to show message */
  showMessage?: boolean;
  /** Whether to show notification */
  showNotification?: boolean;
  /** Message duration (ms) */
  duration?: number;
  /** Custom error message */
  customMessage?: string;
  /** Error level */
  level?: ErrorLevel;
}

// Default configuration
const DEFAULT_CONFIG: ErrorConfig = {
  showMessage: true,
  showNotification: false,
  duration: 5000,
  level: 'error',
};

/**
 * Extract message from error object
 */
export function extractErrorMessage(error: unknown): string {
  if (!error) return 'Unknown error';

  // Axios error
  if (isAxiosError(error)) {
    const axiosError = error as AxiosError<{ message?: string; msg?: string }>;

    // Backend returned error message
    if (axiosError.response?.data?.message) {
      return axiosError.response.data.message;
    }
    if (axiosError.response?.data?.msg) {
      return axiosError.response.data.msg;
    }

    // HTTP status error
    if (axiosError.response?.status) {
      return getHttpStatusMessage(axiosError.response.status);
    }

    // Network error
    if (axiosError.code) {
      return getNetworkErrorMessage(axiosError.code);
    }

    return axiosError.message || 'Request failed';
  }

  // Plain Error object
  if (error instanceof Error) {
    return error.message;
  }

  // String error
  if (typeof error === 'string') {
    return error;
  }

  // Object error
  if (typeof error === 'object' && error !== null) {
    const errorObj = error as Record<string, unknown>;
    if (typeof errorObj.message === 'string') {
      return errorObj.message;
    }
    if (typeof errorObj.msg === 'string') {
      return errorObj.msg;
    }
  }

  return 'Operation failed, please try again later';
}

/**
 * Check if it is an Axios error
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
 * Get message corresponding to HTTP status code
 */
function getHttpStatusMessage(status: number): string {
  const messages: Record<number, string> = {
    400: 'Invalid request parameters',
    401: 'Unauthorized, please login again',
    403: 'Access denied, insufficient permissions',
    404: 'The requested resource does not exist',
    405: 'Request method not allowed',
    408: 'Request timeout',
    409: 'Resource conflict',
    422: 'Request data validation failed',
    429: 'Too many requests, please try again later',
    500: 'Internal server error',
    502: 'Gateway error',
    503: 'Service unavailable',
    504: 'Gateway timeout',
  };

  return messages[status] || `Request failed (${status})`;
}

/**
 * Get message corresponding to network error
 */
function getNetworkErrorMessage(code: string): string {
  const messages: Record<string, string> = {
    ECONNABORTED: 'Request timeout, please check your network connection',
    ENOTFOUND: 'Cannot connect to server',
    ECONNREFUSED: 'Server refused connection',
    ECONNRESET: 'Connection reset',
    ERR_NETWORK: 'Network connection failed, please check your network',
    ERR_CANCELED: 'Request cancelled',
  };

  return messages[code] || 'Network error';
}

/**
 * Unified error handling
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
      title: finalConfig.level === 'error' ? 'Error' : 'Notice',
      content: message,
      duration: finalConfig.duration,
    });
  }

  // Log to console
  console.error('[Error Handler]', error);

  return message;
}

/**
 * Wrap async function to automatically handle errors
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
 * Simplified error handling for try-catch
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
 * Show success message
 */
export function showSuccess(message: string, duration = 3000): void {
  Message.success({ content: message, duration });
}

/**
 * Show warning message
 */
export function showWarning(message: string, duration = 5000): void {
  Message.warning({ content: message, duration });
}

/**
 * Show info message
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
