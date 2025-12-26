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

export function queryDashboardStats() {
  return axios.get<DashboardStats>('/api/dashboard/stats');
}

export function queryContentData() {
  return axios.get<ContentDataRecord[]>('/api/dashboard/content-data');
}

export function queryRecentActivities(params?: { type?: string; limit?: number }) {
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

