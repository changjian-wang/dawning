import axios from '@/api/interceptor';
import type { IPagedData } from '../paged-data';

// ==================== Request Log Interfaces ====================

export interface RequestLog {
  id: string;
  tenantId?: string;
  requestId: string;
  method: string;
  path: string;
  queryString?: string;
  statusCode: number;
  responseTimeMs: number;
  clientIp: string;
  userAgent?: string;
  userId?: string;
  userName?: string;
  requestTime: string;
  requestBodySize?: number;
  responseBodySize?: number;
  exception?: string;
  additionalInfo?: Record<string, unknown>;
}

export interface RequestLogQuery {
  startTime?: string;
  endTime?: string;
  method?: string;
  path?: string;
  statusCode?: number;
  minStatusCode?: number;
  maxStatusCode?: number;
  userId?: string;
  clientIp?: string;
  onlyErrors?: boolean;
  slowRequestThresholdMs?: number;
  page?: number;
  pageSize?: number;
}

export interface RequestStatistics {
  totalRequests: number;
  successRequests: number;
  clientErrors: number;
  serverErrors: number;
  averageResponseTimeMs: number;
  maxResponseTimeMs: number;
  minResponseTimeMs: number;
  p95ResponseTimeMs: number;
  p99ResponseTimeMs: number;
  statusCodeDistribution: Record<string, number>;
  topPaths: Array<{
    path: string;
    requestCount: number;
    averageResponseTimeMs: number;
    errorCount: number;
  }>;
  hourlyRequests: Record<string, number>;
  startTime: string;
  endTime: string;
}

export interface CleanupResult {
  deletedCount: number;
  retentionDays: number;
  message: string;
}

// ==================== Request Log API ====================

/**
 * Get request log list (paged)
 */
export async function getRequestLogs(
  params: RequestLogQuery
): Promise<IPagedData<RequestLog>> {
  const response = await axios.get('/api/request-logs', { params });
  return response.data;
}

/**
 * Get request statistics
 */
export async function getRequestStatistics(
  startTime?: string,
  endTime?: string
): Promise<RequestStatistics> {
  const response = await axios.get('/api/request-logs/statistics', {
    params: { startTime, endTime },
  });
  return response.data;
}

/**
 * Get error request list
 */
export async function getErrorLogs(
  startTime?: string,
  endTime?: string,
  page = 1,
  pageSize = 20
): Promise<IPagedData<RequestLog>> {
  const response = await axios.get('/api/request-logs/errors', {
    params: { startTime, endTime, page, pageSize },
  });
  return response.data;
}

/**
 * Get slow request list
 */
export async function getSlowRequests(
  startTime?: string,
  endTime?: string,
  thresholdMs = 1000,
  page = 1,
  pageSize = 20
): Promise<IPagedData<RequestLog>> {
  const response = await axios.get('/api/request-logs/slow', {
    params: { startTime, endTime, thresholdMs, page, pageSize },
  });
  return response.data;
}

/**
 * Cleanup old request logs
 */
export async function cleanupLogs(
  retentionDays = 30
): Promise<CleanupResult> {
  const response = await axios.delete('/api/request-logs/cleanup', {
    params: { retentionDays },
  });
  return response.data;
}
