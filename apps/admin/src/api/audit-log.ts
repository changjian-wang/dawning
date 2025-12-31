import axios from '@/api/interceptor';
import type { IPagedData } from './paged-data';

// Audit log interface definition
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

// Audit log query parameters
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

// Get audit logs paginated list
export async function getAuditLogs(
  params: AuditLogQueryParams
): Promise<IPagedData<AuditLog>> {
  const response = await axios.get<{ list: AuditLog[]; pagination: any }>(
    '/api/audit-log',
    { params }
  );
  // Interceptor returns { code, message, data }, response.data contains { list, pagination }
  const { list, pagination } = response.data;
  return {
    items: list,
    totalCount: pagination.total,
    pageIndex: pagination.current,
    pageSize: pagination.pageSize,
  };
}

// Get audit log details by ID
export function getAuditLogById(id: string) {
  return axios.get<AuditLog>(`/api/audit-log/${id}`);
}

// Clean up audit logs older than specified days (super_admin only)
export function cleanupOldAuditLogs(daysToKeep: number) {
  return axios.delete<void>(`/api/audit-log/cleanup?daysToKeep=${daysToKeep}`);
}
