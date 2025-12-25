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
              <div class="statistic-title">系统状态</div>
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
              <div class="statistic-title">运行时间</div>
              <div class="statistic-value">{{ uptime }}</div>
            </div>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <div class="custom-statistic">
              <div class="statistic-title">内存使用</div>
              <div class="statistic-value">{{ memoryUsage }}</div>
            </div>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <div class="custom-statistic">
              <div class="statistic-title">线程数</div>
              <div class="statistic-value">{{ threadCount }}</div>
            </div>
          </a-card>
        </a-col>
      </a-row>

      <!-- 健康检查详情 -->
      <a-card class="general-card" title="服务健康检查">
        <template #extra>
          <a-button type="text" :loading="loading" @click="refreshAll">
            <template #icon><icon-refresh /></template>
            刷新
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
          <a-card class="general-card" title="内存详情">
            <a-descriptions :column="1" bordered size="small">
              <a-descriptions-item label="工作集内存">
                {{ metrics?.memory?.workingSet || '-' }}
              </a-descriptions-item>
              <a-descriptions-item label="私有内存">
                {{ metrics?.memory?.privateMemory || '-' }}
              </a-descriptions-item>
              <a-descriptions-item label="虚拟内存">
                {{ metrics?.memory?.virtualMemory || '-' }}
              </a-descriptions-item>
              <a-descriptions-item label="GC 内存">
                {{ metrics?.memory?.gcMemory || '-' }}
              </a-descriptions-item>
            </a-descriptions>
          </a-card>
        </a-col>
        <a-col :xs="24" :md="12">
          <a-card class="general-card" title="CPU & 垃圾回收">
            <a-descriptions :column="1" bordered size="small">
              <a-descriptions-item label="总处理器时间">
                {{ metrics?.cpu?.totalProcessorTime || '-' }}
              </a-descriptions-item>
              <a-descriptions-item label="用户处理器时间">
                {{ metrics?.cpu?.userProcessorTime || '-' }}
              </a-descriptions-item>
              <a-descriptions-item label="Gen0 回收次数">
                {{ metrics?.gc?.gen0Collections ?? '-' }}
              </a-descriptions-item>
              <a-descriptions-item label="Gen1 回收次数">
                {{ metrics?.gc?.gen1Collections ?? '-' }}
              </a-descriptions-item>
              <a-descriptions-item label="Gen2 回收次数">
                {{ metrics?.gc?.gen2Collections ?? '-' }}
              </a-descriptions-item>
            </a-descriptions>
          </a-card>
        </a-col>
      </a-row>

      <!-- 服务端点检查 -->
      <a-card class="general-card" title="服务端点" style="margin-top: 16px">
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
              {{ record.status === 'healthy' ? '正常' : '异常' }}
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
              检查
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
  import {
    healthApi,
    type DetailedHealthStatus,
    type Metrics,
    type ServiceStatus,
  } from '@/api/administration/health';

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
  const healthColumns = reactive([
    { title: '检查项', dataIndex: 'name', width: 150 },
    { title: '状态', dataIndex: 'status', slotName: 'status', width: 120 },
    {
      title: '响应时间',
      dataIndex: 'responseTime',
      slotName: 'responseTime',
      width: 120,
    },
    { title: '详情', dataIndex: 'detail' },
  ]);

  // 服务端点表格列
  const serviceColumns = reactive([
    { title: '服务名称', dataIndex: 'name', width: 150 },
    { title: 'URL', dataIndex: 'url' },
    { title: '状态', dataIndex: 'status', slotName: 'status', width: 100 },
    {
      title: '响应时间',
      dataIndex: 'responseTime',
      slotName: 'responseTime',
      width: 120,
    },
    {
      title: '最后检查',
      dataIndex: 'lastChecked',
      slotName: 'lastChecked',
      width: 180,
    },
    { title: '操作', slotName: 'optional', width: 100 },
  ]);

  // 健康检查数据
  const healthChecks = computed(() => {
    if (!detailedHealth.value?.checks) return [];

    const { api, database, memory } = detailedHealth.value.checks;
    return [
      {
        name: 'API 服务',
        status: api?.status || 'Unknown',
        responseTime: api?.responseTime,
        detail: api?.error || '正常运行',
      },
      {
        name: '数据库连接',
        status: database?.status || 'Unknown',
        responseTime: database?.responseTime,
        detail: database?.error || '连接正常',
      },
      {
        name: '内存状态',
        status: memory?.status || 'Unknown',
        responseTime: undefined,
        detail: memory?.workingSet
          ? `当前: ${memory.workingSet}, 阈值: ${memory.threshold}`
          : memory?.error || '正常',
      },
    ];
  });

  // 预定义的服务端点
  const defaultServices = [
    { name: 'Identity API', url: '/api/health' },
    { name: 'Health - 详细', url: '/api/health/detailed' },
    { name: 'Health - 就绪', url: '/api/health/ready' },
    { name: 'Health - 存活', url: '/api/health/live' },
  ];

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
      defaultServices.map((s) => healthApi.checkService(s.name, s.url))
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
      Message.success(`${service.name} 检查完成`);
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
