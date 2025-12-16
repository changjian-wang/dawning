import axios from 'axios';
import type { IPagedData } from '@/api/paged-data';

// 告警规则 DTO
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

// 创建告警规则请求
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

// 更新告警规则请求
export interface UpdateAlertRuleRequest extends CreateAlertRuleRequest {}

// 告警历史 DTO
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

// 告警历史查询参数
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

// 更新告警状态请求
export interface UpdateAlertStatusRequest {
  status: string;
  resolvedBy?: string;
}

// 告警检查结果
export interface AlertCheckResult {
  checkedAt: string;
  rulesChecked: number;
  alertsTriggered: number;
  notificationsSent: number;
  triggeredAlerts: string[];
  errors: string[];
}

// 告警统计
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

// 指标类型选项
export const metricTypeOptions = [
  { label: 'CPU 使用率', value: 'cpu' },
  { label: '内存使用率', value: 'memory' },
  { label: '响应时间', value: 'response_time' },
  { label: '错误率', value: 'error_rate' },
  { label: '请求数量', value: 'request_count' },
];

// 操作符选项
export const operatorOptions = [
  { label: '大于 (>)', value: 'gt' },
  { label: '大于等于 (>=)', value: 'gte' },
  { label: '小于 (<)', value: 'lt' },
  { label: '小于等于 (<=)', value: 'lte' },
  { label: '等于 (=)', value: 'eq' },
];

// 严重程度选项
export const severityOptions = [
  { label: '信息', value: 'info', color: 'blue' },
  { label: '警告', value: 'warning', color: 'orange' },
  { label: '错误', value: 'error', color: 'red' },
  { label: '严重', value: 'critical', color: 'magenta' },
];

// 告警状态选项
export const alertStatusOptions = [
  { label: '已触发', value: 'triggered', color: 'red' },
  { label: '已确认', value: 'acknowledged', color: 'orange' },
  { label: '已解决', value: 'resolved', color: 'green' },
];

// 通知渠道选项
export const notifyChannelOptions = [
  { label: '邮件', value: 'email' },
  { label: 'Webhook', value: 'webhook' },
];

// API 基础路径
const BASE_URL = '/api/alert';

// ========== 告警规则 API ==========

// 获取所有告警规则
export function getAllRules() {
  return axios.get<AlertRuleDto[]>(`${BASE_URL}/rules`);
}

// 获取启用的告警规则
export function getEnabledRules() {
  return axios.get<AlertRuleDto[]>(`${BASE_URL}/rules/enabled`);
}

// 获取单个告警规则
export function getRuleById(id: number) {
  return axios.get<AlertRuleDto>(`${BASE_URL}/rules/${id}`);
}

// 创建告警规则
export function createRule(data: CreateAlertRuleRequest) {
  return axios.post<AlertRuleDto>(`${BASE_URL}/rules`, data);
}

// 更新告警规则
export function updateRule(id: number, data: UpdateAlertRuleRequest) {
  return axios.put<AlertRuleDto>(`${BASE_URL}/rules/${id}`, data);
}

// 删除告警规则
export function deleteRule(id: number) {
  return axios.delete(`${BASE_URL}/rules/${id}`);
}

// 启用/禁用告警规则
export function setRuleEnabled(id: number, isEnabled: boolean) {
  return axios.patch(`${BASE_URL}/rules/${id}/enabled`, { isEnabled });
}

// ========== 告警历史 API ==========

// 获取告警历史列表（分页）
export function getAlertHistory(params: AlertHistoryQueryParams) {
  return axios.get<IPagedData<AlertHistoryDto>>(`${BASE_URL}/history`, {
    params,
  });
}

// 获取单个告警历史
export function getAlertHistoryById(id: number) {
  return axios.get<AlertHistoryDto>(`${BASE_URL}/history/${id}`);
}

// 更新告警状态
export function updateAlertStatus(id: number, data: UpdateAlertStatusRequest) {
  return axios.patch(`${BASE_URL}/history/${id}/status`, data);
}

// 获取未解决的告警
export function getUnresolvedAlerts() {
  return axios.get<AlertHistoryDto[]>(`${BASE_URL}/history/unresolved`);
}

// ========== 告警检查和统计 API ==========

// 手动触发告警检查
export function triggerAlertCheck() {
  return axios.post<AlertCheckResult>(`${BASE_URL}/check`);
}

// 获取告警统计
export function getAlertStatistics() {
  return axios.get<AlertStatisticsDto>(`${BASE_URL}/statistics`);
}
