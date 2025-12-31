import axios from '@/api/interceptor';
import type { IPagedData } from './paged-data';

export interface SystemLog {
  id: string;
  level: string;
  message: string;
  exception?: string;
  stackTrace?: string;
  source?: string;
  userId?: string;
  username?: string;
  ipAddress?: string;
  userAgent?: string;
  requestPath?: string;
  requestMethod?: string;
  statusCode?: number;
  createdAt: string;
}

export interface SystemLogQueryParams {
  level?: string;
  keyword?: string;
  userId?: string;
  username?: string;
  ipAddress?: string;
  requestPath?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateSystemLogDto {
  level: string;
  message: string;
  exception?: string;
  stackTrace?: string;
  source?: string;
  userId?: string;
  username?: string;
  ipAddress?: string;
  userAgent?: string;
  requestPath?: string;
  requestMethod?: string;
  statusCode?: number;
}

/**
 * Get system log list (paginated)
 */
export async function getSystemLogList(
  params: SystemLogQueryParams
): Promise<IPagedData<SystemLog>> {
  const response = await axios.get('/api/systemlog/paged', { params });
  // response.data is the data processed by interceptor, in IPagedData format
  return response.data as IPagedData<SystemLog>;
}

/**
 * Get system log details by ID
 */
export function getSystemLog(id: string) {
  return axios.get<SystemLog>(`/api/systemlog/${id}`);
}

/**
 * Delete logs before specified date
 */
export function cleanupSystemLogs(beforeDate: string) {
  return axios.delete('/api/systemlog/cleanup', {
    params: { beforeDate },
  });
}

/**
 * Create test log
 */
export function createTestLog(data: CreateSystemLogDto) {
  return axios.post('/api/systemlog/test', data);
}
