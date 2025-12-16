import axios from 'axios';

export interface SystemConfigItem {
  id: string;
  group: string;
  key: string;
  value: string | null;
  description: string | null;
  valueType?: string;
  isReadonly?: boolean;
  updatedAt?: string;
}

// 获取所有配置分组
export function getConfigGroups() {
  return axios.get<{ success: boolean; data: string[] }>(
    '/api/system-config/groups'
  );
}

// 获取分组下的配置
export function getConfigsByGroup(group: string) {
  return axios.get<{ success: boolean; data: SystemConfigItem[] }>(
    `/api/system-config/group/${encodeURIComponent(group)}`
  );
}

// 获取单个配置值
export function getConfigValue(group: string, key: string) {
  return axios.get<{ success: boolean; data: string }>(
    `/api/system-config/${encodeURIComponent(group)}/${encodeURIComponent(key)}`
  );
}

// 设置配置值
export function setConfigValue(
  group: string,
  key: string,
  value: string,
  description?: string
) {
  return axios.post<{ success: boolean }>(
    `/api/system-config/${encodeURIComponent(group)}/${encodeURIComponent(
      key
    )}`,
    { value, description }
  );
}

// 批量更新配置
export function batchUpdateConfigs(configs: SystemConfigItem[]) {
  return axios.post<{ success: boolean }>('/api/system-config/batch', {
    configs,
  });
}

// 删除配置
export function deleteConfig(group: string, key: string) {
  return axios.delete<{ success: boolean }>(
    `/api/system-config/${encodeURIComponent(group)}/${encodeURIComponent(key)}`
  );
}

// 获取配置更新时间戳
export function getConfigTimestamp() {
  return axios.get<{ success: boolean; data: number }>(
    '/api/system-config/timestamp'
  );
}

// 初始化默认配置
export function initDefaultConfigs() {
  return axios.post<{ success: boolean; message: string }>(
    '/api/system-config/init-defaults'
  );
}
