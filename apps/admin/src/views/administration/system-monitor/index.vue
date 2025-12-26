<template>
  <div class="system-monitor">
    <div class="container">
      <Breadcrumb
        :items="['menu.administration', 'menu.administration.systemMonitor']"
      />

      <!-- 系统状态概览 -->
      <a-row :gutter="16" class="stat-cards">
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <div class="custom-statistic">
              <div class="statistic-title">{{ $t('systemMonitor.systemStatus') }}</div>
              <div class="statistic-value" :style="{ color: statusColor }">
                <icon-check-circle-fill v-if="isHealthy" />
                <icon-close-circle-fill v-else />
                <span>{{ systemStatus }}</span>
              </div>
            </div>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <div class="custom-statistic">
              <div class="statistic-title">{{ $t('systemMonitor.uptime') }}</div>
              <div class="statistic-value">{{ uptime }}</div>
            </div>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <div class="custom-statistic">
              <div class="statistic-title">{{ $t('systemMonitor.memoryUsage') }}</div>
              <div class="statistic-value">{{ memoryUsage }}</div>
            </div>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <div class="custom-statistic">
              <div class="statistic-title">{{ $t('systemMonitor.threadCount') }}</div>
              <div class="statistic-value">{{ threadCount }}</div>
            </div>
          </a-card>
        </a-col>
      </a-row>

      <!-- 健康检查详情 -->
      <a-card class="general-card" :title="$t('systemMonitor.healthCheck')">
        <template #extra>
          <a-button type="text" :loading="loading" @click="refreshAll">
            <template #icon><icon-refresh /></template>
            {{ $t('systemMonitor.refresh') }}
          </a-button>
        </template>

        <a-table
          :columns="healthColumns"
          :data="healthChecks"
          :pagination="false"
        >
          <template #status="{ record }">
            <a-tag :color="getStatusColor(record.status)" size="small">
              <template #icon>
                <icon-check-circle-fill v-if="record.status === 'Healthy'" />
                <icon-exclamation-circle-fill
                  v-else-if="record.status === 'Warning'"
                />
                <icon-close-circle-fill v-else />
              </template>
              {{ record.status }}
            </a-tag>
          </template>
          <template #responseTime="{ record }">
            <span :class="getResponseTimeClass(record.responseTime)">
              {{ record.responseTime || '-' }}
            </span>
          </template>
        </a-table>
      </a-card>

      <!-- 性能指标 -->
      <a-row :gutter="16" style="margin-top: 16px">
        <a-col :xs="24" :md="12">
          <a-card class="general-card" :title="$t('systemMonitor.memoryDetails')">
            <a-descriptions :column="1" bordered size="small">
              <a-descriptions-item :label="$t('systemMonitor.memory.workingSet')">
                {{ metrics?.memory?.workingSet || '-' }}
              </a-descriptions-item>
              <a-descriptions-item :label="$t('systemMonitor.memory.privateMemory')">
                {{ metrics?.memory?.privateMemory || '-' }}
              </a-descriptions-item>
              <a-descriptions-item :label="$t('systemMonitor.memory.virtualMemory')">
                {{ metrics?.memory?.virtualMemory || '-' }}
              </a-descriptions-item>
              <a-descriptions-item :label="$t('systemMonitor.memory.gcMemory')">
                {{ metrics?.memory?.gcMemory || '-' }}
              </a-descriptions-item>
            </a-descriptions>
          </a-card>
        </a-col>
        <a-col :xs="24" :md="12">
          <a-card class="general-card" :title="$t('systemMonitor.cpuGc')">
            <a-descriptions :column="1" bordered size="small">
              <a-descriptions-item :label="$t('systemMonitor.cpu.totalProcessorTime')">
                {{ metrics?.cpu?.totalProcessorTime || '-' }}
              </a-descriptions-item>
              <a-descriptions-item :label="$t('systemMonitor.cpu.userProcessorTime')">
                {{ metrics?.cpu?.userProcessorTime || '-' }}
              </a-descriptions-item>
              <a-descriptions-item :label="$t('systemMonitor.gc.gen0Collections')">
                {{ metrics?.gc?.gen0Collections ?? '-' }}
              </a-descriptions-item>
              <a-descriptions-item :label="$t('systemMonitor.gc.gen1Collections')">
                {{ metrics?.gc?.gen1Collections ?? '-' }}
              </a-descriptions-item>
              <a-descriptions-item :label="$t('systemMonitor.gc.gen2Collections')">
                {{ metrics?.gc?.gen2Collections ?? '-' }}
              </a-descriptions-item>
            </a-descriptions>
          </a-card>
        </a-col>
      </a-row>

      <!-- 服务端点检查 -->
      <a-card class="general-card" :title="$t('systemMonitor.serviceEndpoints')" style="margin-top: 16px">
        <a-table :columns="serviceColumns" :data="services" :pagination="false">
          <template #status="{ record }">
            <a-tag
              :color="record.status === 'healthy' ? 'green' : 'red'"
              size="small"
            >
              <template #icon>
                <icon-check-circle-fill v-if="record.status === 'healthy'" />
                <icon-close-circle-fill v-else />
              </template>
              {{ record.status === 'healthy' ? $t('systemMonitor.status.healthy') : $t('systemMonitor.status.unhealthy') }}
            </a-tag>
          </template>
          <template #responseTime="{ record }">
            <span :class="getResponseTimeClass(record.responseTime + 'ms')">
              {{ record.responseTime ? record.responseTime + 'ms' : '-' }}
            </span>
          </template>
          <template #lastChecked="{ record }">
            {{ formatDateTime(record.lastChecked) }}
          </template>
          <template #optional="{ record }">
            <a-button
              type="text"
              size="small"
              @click="checkSingleService(record)"
            >
              <template #icon><icon-sync /></template>
              {{ $t('systemMonitor.check') }}
            </a-button>
          </template>
        </a-table>
      </a-card>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted, onUnmounted } from 'vue';
  import { Message } from '@arco-design/web-vue';
  import { useI18n } from 'vue-i18n';
  import {
    healthApi,
    type DetailedHealthStatus,
    type Metrics,
    type ServiceStatus,
  } from '@/api/administration/health';

  const { t } = useI18n();

  const loading = ref(false);
  const detailedHealth = ref<DetailedHealthStatus | null>(null);
  const metrics = ref<Metrics | null>(null);
  const services = ref<ServiceStatus[]>([]);

  // 自动刷新定时器
  let refreshTimer: number | null = null;

  // 计算属性
  const systemStatus = computed(() => detailedHealth.value?.status || '未知');
  const isHealthy = computed(() => detailedHealth.value?.status === 'Healthy');
  const statusColor = computed(() => (isHealthy.value ? '#00b42a' : '#f53f3f'));
  const uptime = computed(() => detailedHealth.value?.uptime || '-');
  const memoryUsage = computed(() => metrics.value?.memory?.workingSet || '-');
  const threadCount = computed(() => metrics.value?.threads?.count ?? '-');

  // 健康检查表格列
  const healthColumns = computed(() => [
    { title: t('systemMonitor.column.checkItem'), dataIndex: 'name', width: 180 },
    { title: t('systemMonitor.column.status'), dataIndex: 'status', slotName: 'status', width: 90 },
    {
      title: t('systemMonitor.column.responseTime'),
      dataIndex: 'responseTime',
      slotName: 'responseTime',
      width: 150,
    },
    { title: t('systemMonitor.column.detail'), dataIndex: 'detail' },
  ]);

  // 服务端点表格列
  const serviceColumns = computed(() => [
    { title: t('systemMonitor.column.serviceName'), dataIndex: 'name', width: 140 },
    { title: 'URL', dataIndex: 'url', width: 200 },
    { title: t('systemMonitor.column.status'), dataIndex: 'status', slotName: 'status', width: 100 },
    {
      title: t('systemMonitor.column.responseTime'),
      dataIndex: 'responseTime',
      slotName: 'responseTime',
      width: 110,
    },
    {
      title: t('systemMonitor.column.lastChecked'),
      dataIndex: 'lastChecked',
      slotName: 'lastChecked',
      width: 170,
    },
    { title: t('common.actions'), slotName: 'optional', width: 100 },
  ]);

  // 健康检查数据
  const healthChecks = computed(() => {
    if (!detailedHealth.value?.checks) return [];

    const { api, database, memory } = detailedHealth.value.checks;
    return [
      {
        name: t('systemMonitor.service.apiService'),
        status: api?.status || 'Unknown',
        responseTime: api?.responseTime,
        detail: api?.error || t('systemMonitor.detail.runningNormally'),
      },
      {
        name: t('systemMonitor.service.databaseConnection'),
        status: database?.status || 'Unknown',
        responseTime: database?.responseTime,
        detail: database?.error || t('systemMonitor.detail.connectionNormal'),
      },
      {
        name: t('systemMonitor.service.memoryStatus'),
        status: memory?.status || 'Unknown',
        responseTime: undefined,
        detail: memory?.workingSet
          ? `${t('systemMonitor.detail.current')}: ${memory.workingSet}, ${t('systemMonitor.detail.threshold')}: ${memory.threshold}`
          : memory?.error || t('systemMonitor.detail.normal'),
      },
    ];
  });

  // 预定义的服务端点
  const defaultServices = computed(() => [
    { name: 'Identity API', url: '/api/health' },
    { name: t('systemMonitor.service.healthDetailed'), url: '/api/health/detailed' },
    { name: t('systemMonitor.service.healthReady'), url: '/api/health/ready' },
    { name: t('systemMonitor.service.healthLive'), url: '/api/health/live' },
  ]);

  // 获取状态颜色
  function getStatusColor(status: string): string {
    switch (status) {
      case 'Healthy':
        return 'green';
      case 'Warning':
        return 'orange';
      case 'Unhealthy':
        return 'red';
      default:
        return 'gray';
    }
  }

  // 获取响应时间样式类
  function getResponseTimeClass(responseTime?: string): string {
    if (!responseTime) return '';
    const ms = parseInt(responseTime, 10);
    if (ms < 100) return 'response-fast';
    if (ms < 500) return 'response-normal';
    return 'response-slow';
  }

  // 格式化日期时间
  function formatDateTime(dateStr?: string): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleString('zh-CN');
  }

  // 刷新所有数据
  // 检查所有服务端点
  async function checkAllServices() {
    const results = await Promise.all(
      defaultServices.value.map((s) => healthApi.checkService(s.name, s.url))
    );
    services.value = results;
  }

  async function refreshAll() {
    loading.value = true;
    try {
      const [health, metricsData] = await Promise.all([
        healthApi.getDetailedHealth().catch(() => null),
        healthApi.getMetrics().catch(() => null),
      ]);

      detailedHealth.value = health;
      metrics.value = metricsData;

      // 检查所有服务端点
      await checkAllServices();
    } finally {
      loading.value = false;
    }
  }

  // 检查单个服务
  async function checkSingleService(service: ServiceStatus) {
    const index = services.value.findIndex((s) => s.url === service.url);
    if (index !== -1) {
      const result = await healthApi.checkService(service.name, service.url);
      services.value[index] = result;
      Message.success(t('systemMonitor.message.checkComplete', { name: service.name }));
    }
  }

  // 初始化
  onMounted(() => {
    refreshAll();
    // 每30秒自动刷新
    refreshTimer = window.setInterval(refreshAll, 30000);
  });

  onUnmounted(() => {
    if (refreshTimer) {
      clearInterval(refreshTimer);
    }
  });
</script>

<style scoped lang="less">
  .system-monitor {
    .stat-cards {
      margin-bottom: 16px;
    }

    .stat-card {
      height: 100%;

      .custom-statistic {
        .statistic-title {
          color: #86909c;
          font-size: 14px;
          margin-bottom: 8px;
        }

        .statistic-value {
          font-size: 24px;
          font-weight: 600;
          display: flex;
          align-items: center;
          gap: 8px;
        }
      }
    }

    .general-card {
      margin-bottom: 16px;
    }

    .response-fast {
      color: #00b42a;
      font-weight: 500;
    }

    .response-normal {
      color: #ff7d00;
      font-weight: 500;
    }

    .response-slow {
      color: #f53f3f;
      font-weight: 500;
    }
  }
</style>
