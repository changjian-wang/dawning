<template>
  <div class="request-log">
    <div class="container">
      <Breadcrumb
        :items="['menu.gateway', 'menu.gateway.requestLog']"
      />

      <!-- Statistics Cards -->
      <a-row :gutter="16" class="stat-cards">
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <a-statistic
              :title="$t('gateway.requestLog.totalRequests')"
              :value="statistics.totalRequests"
              :precision="0"
            >
              <template #prefix>
                <icon-send class="stat-icon" style="color: rgb(var(--primary-6))" />
              </template>
            </a-statistic>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <a-statistic
              :title="$t('gateway.requestLog.successRate')"
              :value="successRate"
              :precision="2"
              suffix="%"
            >
              <template #prefix>
                <icon-check-circle class="stat-icon" style="color: rgb(var(--green-6))" />
              </template>
            </a-statistic>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <a-statistic
              :title="$t('gateway.requestLog.avgResponseTime')"
              :value="statistics.averageResponseTimeMs"
              :precision="0"
              suffix="ms"
            >
              <template #prefix>
                <icon-clock-circle class="stat-icon" style="color: rgb(var(--blue-6))" />
              </template>
            </a-statistic>
          </a-card>
        </a-col>
        <a-col :xs="24" :sm="12" :md="6">
          <a-card class="stat-card">
            <a-statistic
              :title="$t('gateway.requestLog.errorRequests')"
              :value="errorCount"
              :precision="0"
            >
              <template #prefix>
                <icon-close-circle class="stat-icon" style="color: rgb(var(--red-6))" />
              </template>
            </a-statistic>
          </a-card>
        </a-col>
      </a-row>

      <!-- Filter Card -->
      <a-card class="general-card" :title="$t('gateway.requestLog.title')">
        <a-row :gutter="16" style="margin-bottom: 16px">
          <a-col :xs="24" :sm="12" :md="6">
            <a-range-picker
              v-model="queryParams.timeRange"
              :placeholder="[$t('gateway.requestLog.startTime'), $t('gateway.requestLog.endTime')]"
              style="width: 100%"
              @change="handleTimeRangeChange"
            />
          </a-col>
          <a-col :xs="24" :sm="12" :md="4">
            <a-select
              v-model="queryParams.method"
              :placeholder="$t('gateway.requestLog.method')"
              allow-clear
              style="width: 100%"
              @change="handleSearch"
            >
              <a-option value="GET">GET</a-option>
              <a-option value="POST">POST</a-option>
              <a-option value="PUT">PUT</a-option>
              <a-option value="DELETE">DELETE</a-option>
              <a-option value="PATCH">PATCH</a-option>
            </a-select>
          </a-col>
          <a-col :xs="24" :sm="12" :md="6">
            <a-input
              v-model="queryParams.path"
              :placeholder="$t('gateway.requestLog.pathPlaceholder')"
              allow-clear
              @press-enter="handleSearch"
            />
          </a-col>
          <a-col :xs="24" :sm="12" :md="4">
            <a-select
              v-model="queryParams.statusFilter"
              :placeholder="$t('gateway.requestLog.statusCode')"
              allow-clear
              style="width: 100%"
              @change="handleStatusFilterChange"
            >
              <a-option value="all">{{ $t('gateway.requestLog.allStatus') }}</a-option>
              <a-option value="success">{{ $t('gateway.requestLog.success') }}</a-option>
              <a-option value="error">{{ $t('gateway.requestLog.error') }}</a-option>
              <a-option value="slow">{{ $t('gateway.requestLog.slow') }}</a-option>
            </a-select>
          </a-col>
          <a-col :xs="24" :sm="12" :md="4">
            <a-space>
              <a-button type="primary" @click="handleSearch">
                <template #icon><icon-search /></template>
                {{ $t('common.search') }}
              </a-button>
              <a-button @click="handleReset">
                <template #icon><icon-refresh /></template>
                {{ $t('common.reset') }}
              </a-button>
            </a-space>
          </a-col>
        </a-row>

        <!-- Request Log Table -->
        <a-table
          :columns="columns"
          :data="logs"
          :loading="loading"
          :pagination="pagination"
          :scroll="{ x: 1200 }"
          row-key="id"
          @page-change="onPageChange"
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
          <template #responseTimeMs="{ record }">
            <span :class="getResponseTimeClass(record.responseTimeMs)">
              {{ record.responseTimeMs }}ms
            </span>
          </template>
          <template #requestTime="{ record }">
            {{ formatDateTime(record.requestTime) }}
          </template>
          <template #path="{ record }">
            <a-tooltip :content="record.path">
              <span class="ellipsis-text" style="max-width: 200px; display: inline-block">
                {{ record.path }}
              </span>
            </a-tooltip>
          </template>
          <template #operations="{ record }">
            <a-button type="text" size="small" @click="showDetail(record)">
              <template #icon><icon-eye /></template>
              {{ $t('common.detail') }}
            </a-button>
          </template>
        </a-table>
      </a-card>

      <!-- Detail Modal -->
      <a-modal
        v-model:visible="detailVisible"
        :title="$t('gateway.requestLog.detail')"
        :width="700"
        :footer="false"
      >
        <a-descriptions :column="2" bordered>
          <a-descriptions-item :label="$t('gateway.requestLog.requestId')">
            <a-typography-text copyable>{{ currentLog?.requestId }}</a-typography-text>
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.requestTime')">
            {{ formatDateTime(currentLog?.requestTime) }}
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.method')">
            <a-tag :color="getMethodColor(currentLog?.method || '')">
              {{ currentLog?.method }}
            </a-tag>
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.statusCode')">
            <a-tag :color="getStatusColor(currentLog?.statusCode || 0)">
              {{ currentLog?.statusCode }}
            </a-tag>
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.path')" :span="2">
            <a-typography-text copyable>{{ currentLog?.path }}</a-typography-text>
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.queryString')" :span="2">
            {{ currentLog?.queryString || '-' }}
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.responseTime')">
            <span :class="getResponseTimeClass(currentLog?.responseTimeMs || 0)">
              {{ currentLog?.responseTimeMs }}ms
            </span>
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.clientIp')">
            {{ currentLog?.clientIp }}
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.userId')">
            {{ currentLog?.userName || currentLog?.userId || '-' }}
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.requestBodySize')">
            {{ formatBytes(currentLog?.requestBodySize) }}
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.responseBodySize')">
            {{ formatBytes(currentLog?.responseBodySize) }}
          </a-descriptions-item>
          <a-descriptions-item :label="$t('gateway.requestLog.userAgent')" :span="2">
            <a-typography-text class="user-agent-text">
              {{ currentLog?.userAgent || '-' }}
            </a-typography-text>
          </a-descriptions-item>
          <a-descriptions-item 
            v-if="currentLog?.exception" 
            :label="$t('gateway.requestLog.exception')" 
            :span="2"
          >
            <a-alert type="error">
              <pre class="exception-text">{{ currentLog?.exception }}</pre>
            </a-alert>
          </a-descriptions-item>
          <a-descriptions-item 
            v-if="currentLog?.additionalInfo" 
            :label="$t('gateway.requestLog.additionalInfo')" 
            :span="2"
          >
            <pre class="json-text">{{ JSON.stringify(currentLog?.additionalInfo, null, 2) }}</pre>
          </a-descriptions-item>
        </a-descriptions>
      </a-modal>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { Message } from '@arco-design/web-vue';
import Breadcrumb from '@/components/breadcrumb/index.vue';
import {
  getRequestLogs,
  getRequestStatistics,
  type RequestLog,
  type RequestStatistics,
} from '@/api/gateway/request-log';

const { t } = useI18n();

// State
const loading = ref(false);
const logs = ref<RequestLog[]>([]);
const total = ref(0);
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
const detailVisible = ref(false);
const currentLog = ref<RequestLog | null>(null);

// Query params
const queryParams = reactive({
  timeRange: [] as string[],
  startTime: undefined as string | undefined,
  endTime: undefined as string | undefined,
  method: undefined as string | undefined,
  path: undefined as string | undefined,
  statusFilter: 'all',
  onlyErrors: false,
  slowRequestThresholdMs: undefined as number | undefined,
  page: 1,
  pageSize: 20,
});

// Computed values for statistics
const errorCount = computed(() => statistics.value.clientErrors + statistics.value.serverErrors);
const successRate = computed(() => {
  const total = statistics.value.totalRequests;
  if (total === 0) return 100;
  return ((statistics.value.successRequests / total) * 100);
});

// Pagination
const pagination = computed(() => ({
  current: queryParams.page,
  pageSize: queryParams.pageSize,
  total: total.value,
  showTotal: true,
  showPageSize: true,
}));

// Table columns
const columns = computed(() => [
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
    width: 90,
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
    title: t('gateway.requestLog.responseTime'),
    dataIndex: 'responseTimeMs',
    slotName: 'responseTimeMs',
    width: 120,
  },
  {
    title: t('gateway.requestLog.clientIp'),
    dataIndex: 'clientIp',
    width: 130,
  },
  {
    title: t('common.operations'),
    slotName: 'operations',
    width: 100,
    fixed: 'right',
  },
]);

// Methods
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

const formatBytes = (bytes?: number) => {
  if (!bytes) return '-';
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(2)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(2)} MB`;
};

const handleTimeRangeChange = (value: string[]) => {
  if (value && value.length === 2) {
    queryParams.startTime = value[0];
    queryParams.endTime = value[1];
  } else {
    queryParams.startTime = undefined;
    queryParams.endTime = undefined;
  }
};

const handleStatusFilterChange = (value: string) => {
  queryParams.onlyErrors = value === 'error';
  queryParams.slowRequestThresholdMs = value === 'slow' ? 1000 : undefined;
  handleSearch();
};

const fetchLogs = async () => {
  loading.value = true;
  try {
    const result = await getRequestLogs({
      startTime: queryParams.startTime,
      endTime: queryParams.endTime,
      method: queryParams.method,
      path: queryParams.path,
      onlyErrors: queryParams.onlyErrors || undefined,
      slowRequestThresholdMs: queryParams.slowRequestThresholdMs,
      page: queryParams.page,
      pageSize: queryParams.pageSize,
    });
    logs.value = result.items || [];
    total.value = result.totalCount || 0;
  } catch (error) {
    Message.error(t('common.error'));
  } finally {
    loading.value = false;
  }
};

const fetchStatistics = async () => {
  try {
    const result = await getRequestStatistics(
      queryParams.startTime,
      queryParams.endTime
    );
    statistics.value = result;
  } catch (error) {
    console.error('Failed to fetch statistics:', error);
  }
};

const handleSearch = () => {
  queryParams.page = 1;
  fetchLogs();
  fetchStatistics();
};

const handleReset = () => {
  queryParams.timeRange = [];
  queryParams.startTime = undefined;
  queryParams.endTime = undefined;
  queryParams.method = undefined;
  queryParams.path = undefined;
  queryParams.statusFilter = 'all';
  queryParams.onlyErrors = false;
  queryParams.slowRequestThresholdMs = undefined;
  queryParams.page = 1;
  handleSearch();
};

const onPageChange = (page: number) => {
  queryParams.page = page;
  fetchLogs();
};

const showDetail = (record: RequestLog) => {
  currentLog.value = record;
  detailVisible.value = true;
};

// Lifecycle
onMounted(() => {
  fetchLogs();
  fetchStatistics();
});
</script>

<style lang="less" scoped>
.request-log {
  .stat-cards {
    margin-bottom: 16px;
  }

  .stat-card {
    border-radius: 4px;
  }

  .stat-icon {
    font-size: 20px;
    margin-right: 8px;
  }

  .ellipsis-text {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
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

  .user-agent-text {
    word-break: break-all;
    font-size: 12px;
  }

  .exception-text {
    margin: 0;
    font-size: 12px;
    white-space: pre-wrap;
    word-break: break-all;
  }

  .json-text {
    margin: 0;
    font-size: 12px;
    background-color: var(--color-fill-2);
    padding: 8px;
    border-radius: 4px;
  }
}
</style>
