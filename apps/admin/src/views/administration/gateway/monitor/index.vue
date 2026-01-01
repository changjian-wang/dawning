<template>
  <div class="gateway-monitor">
    <div class="container">
      <Breadcrumb
        :items="['menu.gateway', 'menu.gateway.monitor']"
      />

      <!-- Overview Cards -->
      <a-row :gutter="16" class="stat-cards">
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card stat-card-primary">
            <a-statistic
              :title="$t('gateway.monitor.totalRequests')"
              :value="statistics.totalRequests"
              :precision="0"
            >
              <template #prefix>
                <icon-send class="stat-icon" />
              </template>
            </a-statistic>
            <div class="stat-extra">
              {{ $t('gateway.monitor.last24h') }}
            </div>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card stat-card-success">
            <a-statistic
              :title="$t('gateway.monitor.successRate')"
              :value="successRate"
              :precision="2"
              suffix="%"
            >
              <template #prefix>
                <icon-check-circle class="stat-icon" />
              </template>
            </a-statistic>
            <div class="stat-extra">
              {{ statistics.successRequests }} / {{ statistics.totalRequests }}
            </div>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card stat-card-warning">
            <a-statistic
              :title="$t('gateway.monitor.avgResponseTime')"
              :value="statistics.averageResponseTimeMs"
              :precision="0"
              suffix="ms"
            >
              <template #prefix>
                <icon-clock-circle class="stat-icon" />
              </template>
            </a-statistic>
            <div class="stat-extra">
              {{ $t('gateway.monitor.maxTime') }}: {{ statistics.maxResponseTimeMs }}ms
            </div>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card stat-card-danger">
            <a-statistic
              :title="$t('gateway.monitor.errorRate')"
              :value="errorRate"
              :precision="2"
              suffix="%"
            >
              <template #prefix>
                <icon-close-circle class="stat-icon" />
              </template>
            </a-statistic>
            <div class="stat-extra">
              {{ errorCount }} {{ $t('gateway.monitor.errors') }}
            </div>
          </a-card>
        </a-col>
      </a-row>

      <!-- Time Range Selector -->
      <a-card class="general-card time-filter-card">
        <a-space>
          <span>{{ $t('gateway.monitor.timeRange') }}:</span>
          <a-radio-group v-model="timeRange" type="button" @change="handleTimeRangeChange">
            <a-radio value="1h">{{ $t('gateway.monitor.last1h') }}</a-radio>
            <a-radio value="6h">{{ $t('gateway.monitor.last6h') }}</a-radio>
            <a-radio value="24h">{{ $t('gateway.monitor.last24h') }}</a-radio>
            <a-radio value="7d">{{ $t('gateway.monitor.last7d') }}</a-radio>
          </a-radio-group>
          <a-button type="text" :loading="loading" @click="refreshData">
            <template #icon><icon-refresh /></template>
          </a-button>
        </a-space>
      </a-card>

      <!-- Charts Row -->
      <a-row :gutter="16" style="margin-top: 16px">
        <!-- Request Trend Chart -->
        <a-col :xs="24" :lg="16">
          <a-card class="general-card chart-card" :title="$t('gateway.monitor.requestTrend')">
            <div ref="trendChartRef" class="chart-container"></div>
          </a-card>
        </a-col>

        <!-- Method Distribution -->
        <a-col :xs="24" :lg="8">
          <a-card class="general-card chart-card" :title="$t('gateway.monitor.methodDistribution')">
            <div ref="methodChartRef" class="chart-container"></div>
          </a-card>
        </a-col>
      </a-row>

      <a-row :gutter="16" style="margin-top: 16px">
        <!-- Status Code Distribution -->
        <a-col :xs="24" :lg="8">
          <a-card class="general-card chart-card" :title="$t('gateway.monitor.statusDistribution')">
            <div ref="statusChartRef" class="chart-container"></div>
          </a-card>
        </a-col>

        <!-- Top Paths -->
        <a-col :xs="24" :lg="16">
          <a-card class="general-card" :title="$t('gateway.monitor.topPaths')">
            <a-table
              :columns="topPathColumns"
              :data="statistics.topPaths || []"
              :pagination="false"
              :scroll="{ y: 280 }"
              size="small"
            >
              <template #path="{ record }">
                <a-tooltip :content="record.path">
                  <span class="ellipsis-text">{{ record.path }}</span>
                </a-tooltip>
              </template>
              <template #errorRate="{ record }">
                <a-tag :color="record.errorRate > 5 ? 'red' : record.errorRate > 1 ? 'orange' : 'green'" size="small">
                  {{ record.errorRate.toFixed(2) }}%
                </a-tag>
              </template>
              <template #avgResponseTime="{ record }">
                <span :class="getResponseTimeClass(record.avgResponseTime)">
                  {{ record.avgResponseTime.toFixed(0) }}ms
                </span>
              </template>
            </a-table>
          </a-card>
        </a-col>
      </a-row>

      <!-- Recent Errors -->
      <a-card 
        class="general-card" 
        style="margin-top: 16px"
        :title="$t('gateway.monitor.recentErrors')"
      >
        <template #extra>
          <a-button type="text" size="small" @click="goToLogs">
            {{ $t('gateway.monitor.viewAll') }}
            <template #icon><icon-right /></template>
          </a-button>
        </template>
        <a-table
          :columns="errorColumns"
          :data="recentErrors"
          :loading="errorsLoading"
          :pagination="false"
          size="small"
        >
          <template #method="{ record }">
            <a-tag :color="getMethodColor(record.method)" size="small">
              {{ record.method }}
            </a-tag>
          </template>
          <template #statusCode="{ record }">
            <a-tag :color="getStatusColor(record.statusCode)" size="small">
              {{ record.statusCode }}
            </a-tag>
          </template>
          <template #requestTime="{ record }">
            {{ formatDateTime(record.requestTime) }}
          </template>
          <template #path="{ record }">
            <a-tooltip :content="record.path">
              <span class="ellipsis-text" style="max-width: 300px">{{ record.path }}</span>
            </a-tooltip>
          </template>
        </a-table>
      </a-card>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import * as echarts from 'echarts/core';
import { LineChart, PieChart, BarChart } from 'echarts/charts';
import {
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent,
} from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import Breadcrumb from '@/components/breadcrumb/index.vue';
import {
  getRequestStatistics,
  getErrorLogs,
  type RequestLog,
  type RequestStatistics,
} from '@/api/gateway/request-log';

// Register ECharts components
echarts.use([
  LineChart,
  PieChart,
  BarChart,
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent,
  CanvasRenderer,
]);

const { t } = useI18n();
const router = useRouter();

// State
const loading = ref(false);
const errorsLoading = ref(false);
const timeRange = ref('24h');
const statistics = ref<RequestStatistics>({
  totalRequests: 0,
  successRequests: 0,
  clientErrors: 0,
  serverErrors: 0,
  averageResponseTimeMs: 0,
  maxResponseTimeMs: 0,
  minResponseTimeMs: 0,
  p95ResponseTimeMs: 0,
  p99ResponseTimeMs: 0,
  statusCodeDistribution: {},
  topPaths: [],
  hourlyRequests: {},
  startTime: '',
  endTime: '',
});
const recentErrors = ref<RequestLog[]>([]);

// Chart refs
const trendChartRef = ref<HTMLElement | null>(null);
const methodChartRef = ref<HTMLElement | null>(null);
const statusChartRef = ref<HTMLElement | null>(null);
let trendChart: echarts.ECharts | null = null;
let methodChart: echarts.ECharts | null = null;
let statusChart: echarts.ECharts | null = null;

// Computed
const errorCount = computed(() => statistics.value.clientErrors + statistics.value.serverErrors);
const errorRate = computed(() => {
  if (statistics.value.totalRequests === 0) return 0;
  return (errorCount.value / statistics.value.totalRequests) * 100;
});
const successRate = computed(() => {
  if (statistics.value.totalRequests === 0) return 100;
  return 100 - errorRate.value;
});

// Table columns
const topPathColumns = computed(() => [
  {
    title: t('gateway.monitor.path'),
    dataIndex: 'path',
    slotName: 'path',
    ellipsis: true,
  },
  {
    title: t('gateway.monitor.requestCount'),
    dataIndex: 'count',
    width: 100,
  },
  {
    title: t('gateway.monitor.avgResponseTime'),
    dataIndex: 'avgResponseTime',
    slotName: 'avgResponseTime',
    width: 120,
  },
  {
    title: t('gateway.monitor.errorRate'),
    dataIndex: 'errorRate',
    slotName: 'errorRate',
    width: 100,
  },
]);

const errorColumns = computed(() => [
  {
    title: t('gateway.requestLog.requestTime'),
    dataIndex: 'requestTime',
    slotName: 'requestTime',
    width: 170,
  },
  {
    title: t('gateway.requestLog.method'),
    dataIndex: 'method',
    slotName: 'method',
    width: 80,
  },
  {
    title: t('gateway.requestLog.path'),
    dataIndex: 'path',
    slotName: 'path',
    ellipsis: true,
  },
  {
    title: t('gateway.requestLog.statusCode'),
    dataIndex: 'statusCode',
    slotName: 'statusCode',
    width: 100,
  },
  {
    title: t('gateway.requestLog.clientIp'),
    dataIndex: 'clientIp',
    width: 130,
  },
]);

// Methods
const getTimeParams = () => {
  const now = new Date();
  let startTime: Date;
  
  switch (timeRange.value) {
    case '1h':
      startTime = new Date(now.getTime() - 60 * 60 * 1000);
      break;
    case '6h':
      startTime = new Date(now.getTime() - 6 * 60 * 60 * 1000);
      break;
    case '7d':
      startTime = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
      break;
    default:
      startTime = new Date(now.getTime() - 24 * 60 * 60 * 1000);
  }
  
  return {
    startTime: startTime.toISOString(),
    endTime: now.toISOString(),
  };
};

const getMethodColor = (method: string) => {
  const colors: Record<string, string> = {
    GET: 'arcoblue',
    POST: 'green',
    PUT: 'orange',
    DELETE: 'red',
    PATCH: 'purple',
  };
  return colors[method] || 'gray';
};

const getStatusColor = (statusCode: number) => {
  if (statusCode >= 500) return 'red';
  if (statusCode >= 400) return 'orange';
  if (statusCode >= 300) return 'blue';
  if (statusCode >= 200) return 'green';
  return 'gray';
};

const getResponseTimeClass = (ms: number) => {
  if (ms > 3000) return 'response-time-slow';
  if (ms > 1000) return 'response-time-warning';
  return 'response-time-fast';
};

const formatDateTime = (dateStr?: string) => {
  if (!dateStr) return '-';
  const date = new Date(dateStr);
  return date.toLocaleString();
};

const fetchStatistics = async () => {
  loading.value = true;
  try {
    const { startTime, endTime } = getTimeParams();
    const result = await getRequestStatistics(startTime, endTime);
    statistics.value = result;
    updateCharts();
  } catch (error) {
    console.error('Failed to fetch statistics:', error);
  } finally {
    loading.value = false;
  }
};

const fetchRecentErrors = async () => {
  errorsLoading.value = true;
  try {
    const { startTime, endTime } = getTimeParams();
    const result = await getErrorLogs(startTime, endTime, 1, 5);
    recentErrors.value = result.items || [];
  } catch (error) {
    console.error('Failed to fetch errors:', error);
  } finally {
    errorsLoading.value = false;
  }
};

const initCharts = () => {
  if (trendChartRef.value) {
    trendChart = echarts.init(trendChartRef.value);
  }
  if (methodChartRef.value) {
    methodChart = echarts.init(methodChartRef.value);
  }
  if (statusChartRef.value) {
    statusChart = echarts.init(statusChartRef.value);
  }
};

const updateCharts = () => {
  // Trend chart (line chart for requests by hour)
  if (trendChart && statistics.value.hourlyRequests) {
    const hourlyData = Object.entries(statistics.value.hourlyRequests);
    const hours = hourlyData.map(([hour]) => hour);
    const counts = hourlyData.map(([, count]) => count);

    trendChart.setOption({
      tooltip: {
        trigger: 'axis',
      },
      legend: {
        data: [t('gateway.monitor.requests')],
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true,
      },
      xAxis: {
        type: 'category',
        data: hours,
        axisLabel: {
          rotate: 45,
        },
      },
      yAxis: {
        type: 'value',
      },
      series: [
        {
          name: t('gateway.monitor.requests'),
          type: 'line',
          data: counts,
          smooth: true,
          areaStyle: {
            opacity: 0.3,
          },
          itemStyle: {
            color: 'rgb(var(--primary-6))',
          },
        },
      ],
    });
  }

  // Status code distribution (bar chart) - using statusCodeDistribution
  if (statusChart && statistics.value.statusCodeDistribution) {
    const statusData = Object.entries(statistics.value.statusCodeDistribution);
    const codes = statusData.map(([code]) => code);
    const counts2 = statusData.map(([, count]) => count);
    const colors = statusData.map(([code]) => {
      const num = parseInt(code, 10);
      if (num >= 500) return 'rgb(var(--red-6))';
      if (num >= 400) return 'rgb(var(--orange-6))';
      if (num >= 300) return 'rgb(var(--blue-6))';
      return 'rgb(var(--green-6))';
    });

    // Group by method type for pie chart
    const methodGroups: Record<string, number> = {};
    statusData.forEach(([code, count]) => {
      const num = parseInt(code, 10);
      let category = '2xx';
      if (num >= 500) category = '5xx';
      else if (num >= 400) category = '4xx';
      else if (num >= 300) category = '3xx';
      methodGroups[category] = (methodGroups[category] || 0) + (count as number);
    });

    if (methodChart) {
      const methodData = Object.entries(methodGroups).map(([name, value]) => ({ name, value }));
      methodChart.setOption({
        tooltip: {
          trigger: 'item',
          formatter: '{b}: {c} ({d}%)',
        },
        legend: {
          orient: 'vertical',
          left: 'left',
        },
        series: [
          {
            type: 'pie',
            radius: ['40%', '70%'],
            avoidLabelOverlap: false,
            itemStyle: {
              borderRadius: 4,
            },
            label: {
              show: false,
            },
            emphasis: {
              label: {
                show: true,
                fontSize: 14,
              },
            },
            data: methodData,
          },
        ],
      });
    }

    statusChart.setOption({
      tooltip: {
        trigger: 'axis',
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true,
      },
      xAxis: {
        type: 'category',
        data: codes,
      },
      yAxis: {
        type: 'value',
      },
      series: [
        {
          type: 'bar',
          data: counts2.map((value, index) => ({
            value,
            itemStyle: { color: colors[index] },
          })),
          barWidth: '60%',
        },
      ],
    });
  }
};

const handleTimeRangeChange = () => {
  fetchStatistics();
  fetchRecentErrors();
};

const refreshData = () => {
  fetchStatistics();
  fetchRecentErrors();
};

const goToLogs = () => {
  router.push({ name: 'GatewayRequestLog' });
};

// Handle resize
const handleResize = () => {
  trendChart?.resize();
  methodChart?.resize();
  statusChart?.resize();
};

// Lifecycle
onMounted(() => {
  initCharts();
  fetchStatistics();
  fetchRecentErrors();
  window.addEventListener('resize', handleResize);
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
  trendChart?.dispose();
  methodChart?.dispose();
  statusChart?.dispose();
});

watch(timeRange, handleTimeRangeChange);
</script>

<style lang="less" scoped>
.gateway-monitor {
  .stat-cards {
    margin-bottom: 16px;
  }

  .stat-card {
    border-radius: 4px;
    
    :deep(.arco-statistic-value) {
      font-size: 28px;
    }

    .stat-extra {
      margin-top: 8px;
      font-size: 12px;
      color: var(--color-text-3);
    }
  }

  .stat-card-primary {
    border-left: 3px solid rgb(var(--primary-6));
    
    :deep(.arco-statistic-value) {
      color: rgb(var(--primary-6));
    }
  }

  .stat-card-success {
    border-left: 3px solid rgb(var(--green-6));
    
    :deep(.arco-statistic-value) {
      color: rgb(var(--green-6));
    }
  }

  .stat-card-warning {
    border-left: 3px solid rgb(var(--orange-6));
    
    :deep(.arco-statistic-value) {
      color: rgb(var(--orange-6));
    }
  }

  .stat-card-danger {
    border-left: 3px solid rgb(var(--red-6));
    
    :deep(.arco-statistic-value) {
      color: rgb(var(--red-6));
    }
  }

  .stat-icon {
    font-size: 20px;
    margin-right: 8px;
  }

  .time-filter-card {
    :deep(.arco-card-body) {
      padding: 12px 16px;
    }
  }

  .chart-card {
    min-height: 350px;
  }

  .chart-container {
    width: 100%;
    height: 300px;
  }

  .ellipsis-text {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    display: inline-block;
    max-width: 100%;
  }

  .response-time-fast {
    color: rgb(var(--green-6));
  }

  .response-time-warning {
    color: rgb(var(--orange-6));
  }

  .response-time-slow {
    color: rgb(var(--red-6));
    font-weight: 500;
  }
}
</style>
