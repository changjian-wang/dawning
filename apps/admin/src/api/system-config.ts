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

// Get all configuration groups
export function getConfigGroups() {
  return axios.get<{ success: boolean; data: string[] }>(
    '/api/system-config/groups'
  );
}

// Get configurations by group
export function getConfigsByGroup(group: string) {
  return axios.get<{ success: boolean; data: SystemConfigItem[] }>(
    `/api/system-config/group/${encodeURIComponent(group)}`
  );
}

// Get single configuration value
export function getConfigValue(group: string, key: string) {
  return axios.get<{ success: boolean; data: string }>(
    `/api/system-config/${encodeURIComponent(group)}/${encodeURIComponent(key)}`
  );
}

// Set configuration value
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

// Batch update configurations
export function batchUpdateConfigs(configs: SystemConfigItem[]) {
  return axios.post<{ success: boolean }>('/api/system-config/batch', {
    configs,
  });
}

// Delete configuration
export function deleteConfig(group: string, key: string) {
  return axios.delete<{ success: boolean }>(
    `/api/system-config/${encodeURIComponent(group)}/${encodeURIComponent(key)}`
  );
}

// Get configuration update timestamp
export function getConfigTimestamp() {
  return axios.get<{ success: boolean; data: number }>(
    '/api/system-config/timestamp'
  );
}

// Initialize default configurations
export function initDefaultConfigs() {
  return axios.post<{ success: boolean; message: string }>(
    '/api/system-config/init-defaults'
  );
}
