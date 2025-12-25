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
 * 分页获取系统日志列表
 */
export async function getSystemLogList(
  params: SystemLogQueryParams
): Promise<IPagedData<SystemLog>> {
  const response = await axios.get('/api/systemlog/paged', { params });
  // response.data 就是拦截器处理后的数据，格式是 IPagedData
  return response.data as IPagedData<SystemLog>;
}

/**
 * 根据ID获取系统日志详情
 */
export function getSystemLog(id: string) {
  return axios.get<SystemLog>(`/api/systemlog/${id}`);
}

/**
 * 删除指定日期之前的日志
 */
export function cleanupSystemLogs(beforeDate: string) {
  return axios.delete('/api/systemlog/cleanup', {
    params: { beforeDate },
  });
}

/**
 * 创建测试日志
 */
export function createTestLog(data: CreateSystemLogDto) {
  return axios.post('/api/systemlog/test', data);
}
