<template>
  <a-spin :loading="loading" style="width: 100%">
    <a-card class="general-card" :header-style="{ paddingBottom: '0' }">
      <template #title>
        {{ $t('workplace.gatewayMonitor') }}
      </template>
      <template #extra>
        <a-link @click="fetchData()">{{ $t('workplace.refresh') }}</a-link>
      </template>
      <a-grid :cols="24" :row-gap="16" class="monitor-grid">
        <a-grid-item :span="{ xs: 12, sm: 12, md: 8, lg: 6, xl: 6 }">
          <a-statistic
            :title="$t('workplace.requestsPerMinute')"
            :value="realtimeData.requestsPerMinute"
            :value-from="0"
            animation
          >
            <template #prefix>
              <icon-thunderbolt class="stat-icon" style="color: #165dff" />
            </template>
            <template #suffix>
              <span class="unit">/min</span>
            </template>
          </a-statistic>
        </a-grid-item>
        <a-grid-item :span="{ xs: 12, sm: 12, md: 8, lg: 6, xl: 6 }">
          <a-statistic
            :title="$t('workplace.requestsPerHour')"
            :value="realtimeData.requestsPerHour"
            :value-from="0"
            animation
          >
            <template #prefix>
              <icon-sync class="stat-icon" style="color: #722ed1" />
            </template>
            <template #suffix>
              <span class="unit">/hour</span>
            </template>
          </a-statistic>
        </a-grid-item>
        <a-grid-item :span="{ xs: 12, sm: 12, md: 8, lg: 6, xl: 6 }">
          <a-statistic
            :title="$t('workplace.avgResponseTime')"
            :value="realtimeData.averageResponseTimeMs"
            :precision="1"
            :value-from="0"
            animation
          >
            <template #prefix>
              <icon-clock-circle class="stat-icon" style="color: #0fc6c2" />
            </template>
            <template #suffix>
              <span class="unit">ms</span>
            </template>
          </a-statistic>
        </a-grid-item>
        <a-grid-item :span="{ xs: 12, sm: 12, md: 8, lg: 6, xl: 6 }">
          <a-statistic
            :title="$t('workplace.errorsPerHour')"
            :value="realtimeData.errorsPerHour"
            :value-from="0"
            animation
            :value-style="{
              color: realtimeData.errorsPerHour > 0 ? '#F53F3F' : '#00B42A',
            }"
          >
            <template #prefix>
              <icon-exclamation-circle
                class="stat-icon"
                :style="{
                  color: realtimeData.errorsPerHour > 0 ? '#F53F3F' : '#00B42A',
                }"
              />
            </template>
          </a-statistic>
        </a-grid-item>
        <a-grid-item :span="{ xs: 12, sm: 12, md: 8, lg: 6, xl: 6 }">
          <a-statistic
            :title="$t('workplace.memoryUsage')"
            :value="realtimeData.memoryUsageMb"
            :precision="1"
            :value-from="0"
            animation
          >
            <template #prefix>
              <icon-storage class="stat-icon" style="color: #f77234" />
            </template>
            <template #suffix>
              <span class="unit">MB</span>
            </template>
          </a-statistic>
        </a-grid-item>
        <a-grid-item :span="{ xs: 12, sm: 12, md: 8, lg: 6, xl: 6 }">
          <a-statistic
            :title="$t('workplace.threadCount')"
            :value="realtimeData.threadCount"
            :value-from="0"
            animation
          >
            <template #prefix>
              <icon-branch class="stat-icon" style="color: #14c9c9" />
            </template>
          </a-statistic>
        </a-grid-item>
        <a-grid-item :span="{ xs: 24, sm: 24, md: 24, lg: 12, xl: 12 }">
          <div class="uptime-info">
            <icon-check-circle-fill
              class="uptime-icon"
              style="color: #00b42a"
            />
            <span class="uptime-label">{{ $t('workplace.uptime') }}:</span>
            <span class="uptime-value">{{
              formatUptime(realtimeData.uptime)
            }}</span>
          </div>
        </a-grid-item>
      </a-grid>
    </a-card>
  </a-spin>
</template>

<script lang="ts" setup>
  import { ref, onMounted, onUnmounted } from 'vue';
  import {
    queryRealtimeMonitoring,
    RealtimeMonitoringData,
  } from '@/api/dashboard';
  import useLoading from '@/hooks/loading';

  const { loading, setLoading } = useLoading();

  const realtimeData = ref<RealtimeMonitoringData>({
    requestsPerMinute: 0,
    requestsPerHour: 0,
    errorsPerMinute: 0,
    errorsPerHour: 0,
    averageResponseTimeMs: 0,
    memoryUsageMb: 0,
    managedMemoryMb: 0,
    threadCount: 0,
    uptime: '00:00:00',
    timestamp: '',
  });

  let refreshTimer: number | null = null;

  const formatUptime = (uptime: string): string => {
    if (!uptime) return '-';
    // Parse .NET TimeSpan format: "d.hh:mm:ss.fffffff" or "hh:mm:ss.fffffff"
    const match = uptime.match(/^(?:(\d+)\.)?(\d{2}):(\d{2}):(\d{2})/);
    if (!match) return uptime;
    const [, days, hours, minutes, seconds] = match;
    const parts = [];
    if (days && parseInt(days, 10) > 0) {
      parts.push(`${days}d`);
    }
    parts.push(`${hours}h ${minutes}m ${seconds}s`);
    return parts.join(' ');
  };

  const fetchData = async () => {
    try {
      setLoading(true);
      const { data } = await queryRealtimeMonitoring();
      if (data) {
        realtimeData.value = data;
      }
    } catch (err) {
      // Handle error silently
    } finally {
      setLoading(false);
    }
  };

  onMounted(() => {
    fetchData();
    // Auto refresh every 30 seconds
    refreshTimer = window.setInterval(fetchData, 30000);
  });

  onUnmounted(() => {
    if (refreshTimer) {
      clearInterval(refreshTimer);
    }
  });
</script>

<style scoped lang="less">
  .general-card {
    border-radius: 4px;
  }

  .monitor-grid {
    padding: 16px 0;
  }

  .stat-icon {
    font-size: 22px !important;
    margin-right: 10px;
  }

  :deep(.arco-statistic) {
    .arco-statistic-title {
      font-size: 13px;
      margin-bottom: 10px;
      color: var(--color-text-2);
    }

    .arco-statistic-content {
      display: flex;
      align-items: center;
    }

    .arco-statistic-value {
      font-size: 26px;
      font-weight: 600;
      line-height: 1.2;
    }

    .arco-statistic-value-prefix {
      display: flex;
      align-items: center;
    }

    .arco-statistic-value-suffix {
      margin-left: 4px;
    }
  }

  .unit {
    font-size: 13px;
    color: var(--color-text-3);
  }

  .uptime-info {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 14px 16px;
    background: var(--color-fill-1);
    border-radius: 6px;
  }

  .uptime-icon {
    font-size: 20px !important;
  }

  .uptime-label {
    color: var(--color-text-2);
    font-size: 14px;
  }

  .uptime-value {
    color: var(--color-text-1);
    font-weight: 600;
    font-size: 16px;
  }
</style>
