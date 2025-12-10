import axios from 'axios';
import type { IPagedData } from './paged-data';

// 审计日志接口定义
export interface AuditLog {
  id: string;
  userId?: string;
  username?: string;
  action: string;
  entityType?: string;
  entityId?: string;
  description?: string;
  ipAddress?: string;
  userAgent?: string;
  requestPath?: string;
  requestMethod?: string;
  statusCode?: number;
  oldValues?: any;
  newValues?: any;
  createdAt: string;
}

// 审计日志查询参数
export interface AuditLogQueryParams {
  page?: number;
  pageSize?: number;
  userId?: string;
  username?: string;
  action?: string;
  entityType?: string;
  entityId?: string;
  ipAddress?: string;
  startDate?: string;
  endDate?: string;
}

// 获取审计日志分页列表
export async function getAuditLogs(
  params: AuditLogQueryParams
): Promise<IPagedData<AuditLog>> {
  const response = await axios.get('/api/audit-log', { params });
  // 拦截器返回 { code, message, data }，response.data 包含 { list, pagination }
  const { list, pagination } = response.data;
  return {
    items: list,
    totalCount: pagination.total,
    pageIndex: pagination.current,
    pageSize: pagination.pageSize,
  };
}

// 根据ID获取审计日志详情
export function getAuditLogById(id: string) {
  return axios.get<AuditLog>(`/api/audit-log/${id}`);
}

// 清理指定天数之前的审计日志（仅super_admin）
export function cleanupOldAuditLogs(daysToKeep: number) {
  return axios.delete(`/api/audit-log/cleanup?daysToKeep=${daysToKeep}`);
}
