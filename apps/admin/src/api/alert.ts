import axios from 'axios';
import type { IPagedData } from '@/api/paged-data';

// Alert rule DTO
export interface AlertRuleDto {
  id: number;
  name: string;
  description?: string;
  metricType: string;
  operator: string;
  threshold: number;
  durationSeconds: number;
  severity: string;
  isEnabled: boolean;
  notifyChannels: string[];
  notifyEmails?: string;
  webhookUrl?: string;
  cooldownMinutes: number;
  lastTriggeredAt?: string;
  createdAt: string;
}

// Create alert rule request
export interface CreateAlertRuleRequest {
  name: string;
  description?: string;
  metricType: string;
  operator: string;
  threshold: number;
  durationSeconds: number;
  severity: string;
  isEnabled: boolean;
  notifyChannels: string[];
  notifyEmails?: string;
  webhookUrl?: string;
  cooldownMinutes: number;
}

// Update alert rule request
export type UpdateAlertRuleRequest = CreateAlertRuleRequest;

// Alert history DTO
export interface AlertHistoryDto {
  id: number;
  ruleId: number;
  ruleName: string;
  metricType: string;
  metricValue: number;
  threshold: number;
  severity: string;
  message?: string;
  status: string;
  triggeredAt: string;
  acknowledgedAt?: string;
  acknowledgedBy?: string;
  resolvedAt?: string;
  resolvedBy?: string;
  notifySent: boolean;
}

// Alert history query parameters
export interface AlertHistoryQueryParams {
  ruleId?: number;
  metricType?: string;
  severity?: string;
  status?: string;
  startTime?: string;
  endTime?: string;
  page?: number;
  pageSize?: number;
}

// Update alert status request
export interface UpdateAlertStatusRequest {
  status: string;
  resolvedBy?: string;
}

// Alert check result
export interface AlertCheckResult {
  checkedAt: string;
  rulesChecked: number;
  alertsTriggered: number;
  notificationsSent: number;
  triggeredAlerts: string[];
  errors: string[];
}

// Alert statistics
export interface AlertStatisticsDto {
  totalRules: number;
  enabledRules: number;
  totalAlertsToday: number;
  unresolvedAlerts: number;
  criticalAlerts: number;
  warningAlerts: number;
  alertsByMetricType: Record<string, number>;
  alertsBySeverity: Record<string, number>;
}

// Metric type options
export const metricTypeOptions = [
  { label: 'CPU Usage', value: 'cpu' },
  { label: 'Memory Usage', value: 'memory' },
  { label: 'Response Time', value: 'response_time' },
  { label: 'Error Rate', value: 'error_rate' },
  { label: 'Request Count', value: 'request_count' },
];

// Operator options
export const operatorOptions = [
  { label: 'Greater than (>)', value: 'gt' },
  { label: 'Greater than or equal (>=)', value: 'gte' },
  { label: 'Less than (<)', value: 'lt' },
  { label: 'Less than or equal (<=)', value: 'lte' },
  { label: 'Equal (=)', value: 'eq' },
];

// Severity options
export const severityOptions = [
  { label: 'Info', value: 'info', color: 'blue' },
  { label: 'Warning', value: 'warning', color: 'orange' },
  { label: 'Error', value: 'error', color: 'red' },
  { label: 'Critical', value: 'critical', color: 'magenta' },
];

// Alert status options
export const alertStatusOptions = [
  { label: 'Triggered', value: 'triggered', color: 'red' },
  { label: 'Acknowledged', value: 'acknowledged', color: 'orange' },
  { label: 'Resolved', value: 'resolved', color: 'green' },
];

// Notification channel options
export const notifyChannelOptions = [
  { label: 'Email', value: 'email' },
  { label: 'Webhook', value: 'webhook' },
];

// API base path
const BASE_URL = '/api/admin/alert';

// ========== Alert Rules API ==========

// Get all alert rules
export function getAllRules() {
  return axios.get<AlertRuleDto[]>(`${BASE_URL}/rules`);
}

// Get enabled alert rules
export function getEnabledRules() {
  return axios.get<AlertRuleDto[]>(`${BASE_URL}/rules/enabled`);
}

// Get single alert rule
export function getRuleById(id: number) {
  return axios.get<AlertRuleDto>(`${BASE_URL}/rules/${id}`);
}

// Create alert rule
export function createRule(data: CreateAlertRuleRequest) {
  return axios.post<AlertRuleDto>(`${BASE_URL}/rules`, data);
}

// Update alert rule
export function updateRule(id: number, data: UpdateAlertRuleRequest) {
  return axios.put<AlertRuleDto>(`${BASE_URL}/rules/${id}`, data);
}

// Delete alert rule
export function deleteRule(id: number) {
  return axios.delete(`${BASE_URL}/rules/${id}`);
}

// Enable/disable alert rule
export function setRuleEnabled(id: number, isEnabled: boolean) {
  return axios.patch(`${BASE_URL}/rules/${id}/enabled`, { isEnabled });
}

// ========== Alert History API ==========

// Get alert history list (paginated)
export function getAlertHistory(params: AlertHistoryQueryParams) {
  return axios.get<IPagedData<AlertHistoryDto>>(`${BASE_URL}/history`, {
    params,
  });
}

// Get single alert history
export function getAlertHistoryById(id: number) {
  return axios.get<AlertHistoryDto>(`${BASE_URL}/history/${id}`);
}

// Update alert status
export function updateAlertStatus(id: number, data: UpdateAlertStatusRequest) {
  return axios.patch(`${BASE_URL}/history/${id}/status`, data);
}

// Get unresolved alerts
export function getUnresolvedAlerts() {
  return axios.get<AlertHistoryDto[]>(`${BASE_URL}/history/unresolved`);
}

// ========== Alert Check and Statistics API ==========

// Manually trigger alert check
export function triggerAlertCheck() {
  return axios.post<AlertCheckResult>(`${BASE_URL}/check`);
}

// Get alert statistics
export function getAlertStatistics() {
  return axios.get<AlertStatisticsDto>(`${BASE_URL}/statistics`);
}
