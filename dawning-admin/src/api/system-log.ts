import axios from 'axios';
import type { PagedResult } from '@/types/global';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

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
export function getSystemLogList(params: SystemLogQueryParams) {
  return axios.get<PagedResult<SystemLog>>(`${API_BASE_URL}/api/systemlog/paged`, {
    params,
  });
}

/**
 * 根据ID获取系统日志详情
 */
export function getSystemLog(id: string) {
  return axios.get<SystemLog>(`${API_BASE_URL}/api/systemlog/${id}`);
}

/**
 * 删除指定日期之前的日志
 */
export function cleanupSystemLogs(beforeDate: string) {
  return axios.delete(`${API_BASE_URL}/api/systemlog/cleanup`, {
    params: { beforeDate },
  });
}

/**
 * 创建测试日志
 */
export function createTestLog(data: CreateSystemLogDto) {
  return axios.post(`${API_BASE_URL}/api/systemlog/test`, data);
}
