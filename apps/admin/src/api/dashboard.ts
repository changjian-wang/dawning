import axios from '@/api/interceptor';
import type { TableData } from '@arco-design/web-vue/es/table/interface';

export interface ContentDataRecord {
  x: string;
  y: number;
}

export interface DashboardStats {
  totalUsers: number;
  totalRoles: number;
  todayAuditLogs: number;
  totalApplications: number;
  growthRate: number;
}

export interface CategoryItem {
  name: string;
  value: number;
}

export interface CategoriesData {
  categories: CategoryItem[];
  total: number;
}

export interface ActivityItem {
  key: number;
  title: string;
  description: string;
  time: string;
  user: string;
}

export interface AnnouncementItem {
  type: string;
  label: string;
  content: string;
}

// Gateway monitoring interfaces
export interface RealtimeMonitoringData {
  requestsPerMinute: number;
  requestsPerHour: number;
  errorsPerMinute: number;
  errorsPerHour: number;
  averageResponseTimeMs: number;
  memoryUsageMb: number;
  managedMemoryMb: number;
  threadCount: number;
  uptime: string;
  timestamp: string;
}

export interface RequestStatistics {
  totalRequests: number;
  successfulRequests: number;
  successRequests?: number;
  clientErrors: number;
  serverErrors: number;
  averageResponseTimeMs: number;
  maxResponseTimeMs: number;
  minResponseTimeMs: number;
  requestsByMethod: Record<string, number>;
  requestsByStatusCode: Record<string, number>;
  topPaths: {
    path: string;
    count?: number;
    requestCount?: number;
    avgResponseTimeMs?: number;
    averageResponseTimeMs?: number;
  }[];
}

export interface SystemPerformanceMetrics {
  processId: number;
  processName: string;
  startTime: string;
  uptime: string;
  workingSetMemoryMb: number;
  privateMemoryMb: number;
  virtualMemoryMb: number;
  managedMemoryMb: number;
  gen0Collections: number;
  gen1Collections: number;
  gen2Collections: number;
  totalAllocatedMb: number;
  threadCount: number;
  handleCount: number;
  timestamp: string;
}

export function queryDashboardStats() {
  return axios.get<DashboardStats>('/api/dashboard/stats');
}

export function queryContentData() {
  return axios.get<ContentDataRecord[]>('/api/dashboard/content-data');
}

export function queryRecentActivities(params?: {
  type?: string;
  limit?: number;
}) {
  return axios.get<ActivityItem[]>('/api/dashboard/recent-activities', {
    params,
  });
}

export function queryCategories() {
  return axios.get<CategoriesData>('/api/dashboard/categories');
}

export function queryAnnouncements() {
  return axios.get<AnnouncementItem[]>('/api/dashboard/announcements');
}

// Gateway monitoring APIs
export function queryRealtimeMonitoring() {
  return axios.get<RealtimeMonitoringData>('/api/monitoring/realtime');
}

export function queryRequestStatistics(startTime?: string, endTime?: string) {
  return axios.get<RequestStatistics>('/api/monitoring/statistics', {
    params: { startTime, endTime },
  });
}

export function queryPerformanceMetrics() {
  return axios.get<SystemPerformanceMetrics>('/api/monitoring/performance');
}

// Keep for backward compatibility but mark as deprecated
export interface PopularRecord {
  key: number;
  clickNumber: string;
  title: string;
  increases: number;
}

export function queryPopularList(params: { type: string }) {
  return axios.get<TableData[]>('/api/dashboard/recent-activities', {
    params,
  });
}
