<template>
  <div class="system-log">
    <a-card class="general-card" :title="$t('menu.administration.systemLog')">
      <!-- 搜索表单 -->
      <a-row :gutter="16" style="margin-bottom: 16px">
        <a-col :span="6">
          <a-select
            v-model="queryParams.level"
            :placeholder="$t('systemLog.level')"
            allow-clear
            @change="handleSearch"
          >
            <a-option value="">{{ $t('common.all') }}</a-option>
            <a-option value="Information">Information</a-option>
            <a-option value="Warning">Warning</a-option>
            <a-option value="Error">Error</a-option>
          </a-select>
        </a-col>
        <a-col :span="6">
          <a-input
            v-model="queryParams.keyword"
            :placeholder="$t('systemLog.keyword')"
            allow-clear
            @press-enter="handleSearch"
          >
            <template #prefix>
              <icon-search />
            </template>
          </a-input>
        </a-col>
        <a-col :span="6">
          <a-input
            v-model="queryParams.username"
            :placeholder="$t('systemLog.username')"
            allow-clear
            @press-enter="handleSearch"
          >
            <template #prefix>
              <icon-user />
            </template>
          </a-input>
        </a-col>
        <a-col :span="6">
          <a-space>
            <a-button type="primary" @click="handleSearch">
              <template #icon>
                <icon-search />
              </template>
              {{ $t('common.search') }}
            </a-button>
            <a-button @click="handleReset">
              <template #icon>
                <icon-refresh />
              </template>
              {{ $t('common.reset') }}
            </a-button>
          </a-space>
        </a-col>
      </a-row>

      <!-- 高级搜索 -->
      <a-collapse :default-active-key="[]" style="margin-bottom: 16px">
        <a-collapse-item key="advanced" :header="$t('common.advancedSearch')">
          <a-row :gutter="16">
            <a-col :span="8">
              <a-form-item :label="$t('systemLog.ipAddress')">
                <a-input
                  v-model="queryParams.ipAddress"
                  :placeholder="$t('systemLog.ipAddress')"
                  allow-clear
                />
              </a-form-item>
            </a-col>
            <a-col :span="8">
              <a-form-item :label="$t('systemLog.requestPath')">
                <a-input
                  v-model="queryParams.requestPath"
                  :placeholder="$t('systemLog.requestPath')"
                  allow-clear
                />
              </a-form-item>
            </a-col>
            <a-col :span="8">
              <a-form-item :label="$t('systemLog.dateRange')">
                <a-range-picker
                  v-model="dateRange"
                  show-time
                  format="YYYY-MM-DD HH:mm:ss"
                  @change="handleDateRangeChange"
                />
              </a-form-item>
            </a-col>
          </a-row>
        </a-collapse-item>
      </a-collapse>

      <!-- 操作按钮 -->
      <a-row style="margin-bottom: 16px">
        <a-col :span="12">
          <a-space>
            <a-button type="primary" status="danger" @click="handleCleanup">
              <template #icon>
                <icon-delete />
              </template>
              {{ $t('systemLog.cleanup') }}
            </a-button>
          </a-space>
        </a-col>
      </a-row>

      <!-- 数据表格 -->
      <a-table
        row-key="id"
        :columns="columns"
        :data="data"
        :pagination="pagination"
        :loading="loading"
        :bordered="false"
        :stripe="true"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      >
        <template #level="{ record }">
          <a-tag
            :color="
              record.level === 'Error'
                ? 'red'
                : record.level === 'Warning'
                ? 'orange'
                : 'blue'
            "
          >
            {{ record.level }}
          </a-tag>
        </template>

        <template #message="{ record }">
          <div class="message-cell">
            {{ record.message }}
          </div>
        </template>

        <template #exception="{ record }">
          <a-tooltip v-if="record.exception" :content="record.exception">
            <icon-exclamation-circle-fill style="color: #f53f3f" />
          </a-tooltip>
          <span v-else>-</span>
        </template>

        <template #requestInfo="{ record }">
          <div v-if="record.requestMethod && record.requestPath">
            <a-tag>{{ record.requestMethod }}</a-tag>
            <span style="margin-left: 4px">{{ record.requestPath }}</span>
          </div>
          <span v-else>-</span>
        </template>

        <template #userInfo="{ record }">
          <div v-if="record.username">
            {{ record.username }}
            <div style="font-size: 12px; color: #86909c">
              {{ record.ipAddress || '-' }}
            </div>
          </div>
          <span v-else>{{ $t('common.system') }}</span>
        </template>

        <template #createdAt="{ record }">
          {{ formatDateTime(record.createdAt) }}
        </template>

        <template #operations="{ record }">
          <a-button type="text" size="small" @click="handleDetail(record)">
            <template #icon>
              <icon-eye />
            </template>
            {{ $t('common.detail') }}
          </a-button>
        </template>
      </a-table>
    </a-card>

    <!-- 详情模态框 -->
    <a-modal
      v-model:visible="detailVisible"
      :title="$t('systemLog.detail')"
      :width="900"
      :footer="false"
      unmount-on-close
    >
      <div v-if="currentLog" class="detail-content">
        <div class="detail-row">
          <span class="label">{{ $t('systemLog.level') }}</span>
          <span class="value">
            <a-tag
              :color="
                currentLog.level === 'Error'
                  ? 'red'
                  : currentLog.level === 'Warning'
                  ? 'orange'
                  : 'blue'
              "
            >
              {{ currentLog.level }}
            </a-tag>
          </span>
        </div>
        <div class="detail-row">
          <span class="label">{{ $t('systemLog.message') }}</span>
          <span class="value">{{ currentLog.message }}</span>
        </div>
        <div v-if="currentLog.exception" class="detail-row">
          <span class="label">{{ $t('systemLog.exception') }}</span>
          <span class="value">
            <a-textarea
              :model-value="currentLog.exception"
              :auto-size="{ minRows: 3, maxRows: 10 }"
              readonly
            />
          </span>
        </div>
        <div v-if="currentLog.stackTrace" class="detail-row">
          <span class="label">{{ $t('systemLog.stackTrace') }}</span>
          <span class="value">
            <a-textarea
              :model-value="currentLog.stackTrace"
              :auto-size="{ minRows: 5, maxRows: 15 }"
              readonly
            />
          </span>
        </div>
        <div v-if="currentLog.source" class="detail-row">
          <span class="label">{{ $t('systemLog.source') }}</span>
          <span class="value">{{ currentLog.source }}</span>
        </div>
        <div class="detail-row">
          <span class="label">{{ $t('systemLog.requestMethod') }}</span>
          <span class="value">{{ currentLog.requestMethod || '-' }}</span>
        </div>
        <div class="detail-row">
          <span class="label">{{ $t('systemLog.requestPath') }}</span>
          <span class="value">{{ currentLog.requestPath || '-' }}</span>
        </div>
        <div class="detail-row">
          <span class="label">{{ $t('systemLog.statusCode') }}</span>
          <span class="value">{{ currentLog.statusCode || '-' }}</span>
        </div>
        <div class="detail-row">
          <span class="label">{{ $t('systemLog.username') }}</span>
          <span class="value">{{ currentLog.username || '-' }}</span>
        </div>
        <div class="detail-row">
          <span class="label">{{ $t('systemLog.ipAddress') }}</span>
          <span class="value">{{ currentLog.ipAddress || '-' }}</span>
        </div>
        <div v-if="currentLog.userAgent" class="detail-row">
          <span class="label">{{ $t('systemLog.userAgent') }}</span>
          <span class="value">{{ currentLog.userAgent }}</span>
        </div>
        <div class="detail-row">
          <span class="label">{{ $t('common.createdAt') }}</span>
          <span class="value">{{ formatDateTime(currentLog.createdAt) }}</span>
        </div>
      </div>
    </a-modal>

    <!-- 清理日志模态框 -->
    <a-modal
      v-model:visible="cleanupVisible"
      :title="$t('systemLog.cleanup')"
      @ok="handleCleanupConfirm"
      @cancel="cleanupVisible = false"
    >
      <a-form :model="cleanupForm">
        <a-form-item :label="$t('systemLog.cleanupBeforeDate')">
          <a-date-picker
            v-model="cleanupForm.beforeDate"
            show-time
            format="YYYY-MM-DD HH:mm:ss"
            :placeholder="$t('systemLog.selectDate')"
          />
        </a-form-item>
      </a-form>
      <a-alert type="warning" style="margin-top: 16px">
        {{ $t('systemLog.cleanupWarning') }}
      </a-alert>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted } from 'vue';
  import { useI18n } from 'vue-i18n';
  import { Message } from '@arco-design/web-vue';
  import dayjs from 'dayjs';
  import {
    getSystemLogList,
    cleanupSystemLogs,
    type SystemLog,
    type SystemLogQueryParams,
  } from '@/api/system-log';

  const { t } = useI18n();

  // 查询参数
  const queryParams = reactive<SystemLogQueryParams>({
    level: '',
    keyword: '',
    username: '',
    ipAddress: '',
    requestPath: '',
    startDate: '',
    endDate: '',
    page: 1,
    pageSize: 20,
  });

  // 日期范围
  const dateRange = ref<[string, string] | []>([]);

  // 表格列定义
  const columns = computed(() => [
    {
      title: t('systemLog.level'),
      dataIndex: 'level',
      slotName: 'level',
      width: 100,
    },
    {
      title: t('systemLog.message'),
      dataIndex: 'message',
      slotName: 'message',
      width: 300,
      ellipsis: true,
      tooltip: true,
    },
    {
      title: t('systemLog.exception'),
      dataIndex: 'exception',
      slotName: 'exception',
      width: 80,
      align: 'center' as const,
    },
    {
      title: t('systemLog.requestInfo'),
      slotName: 'requestInfo',
      width: 250,
    },
    {
      title: t('systemLog.userInfo'),
      slotName: 'userInfo',
      width: 180,
    },
    {
      title: t('common.createdAt'),
      dataIndex: 'createdAt',
      slotName: 'createdAt',
      width: 180,
    },
    {
      title: t('common.actions'),
      slotName: 'operations',
      width: 100,
      fixed: 'right' as const,
    },
  ]);

  // 表格数据
  const data = ref<SystemLog[]>([]);
  const loading = ref(false);

  // 分页配置
  const pagination = reactive({
    current: 1,
    pageSize: 20,
    total: 0,
    showTotal: true,
    showPageSize: true,
  });

  // 详情
  const detailVisible = ref(false);
  const currentLog = ref<SystemLog | null>(null);

  // 清理
  const cleanupVisible = ref(false);
  const cleanupForm = reactive({
    beforeDate: '',
  });

  // 加载数据
  const fetchData = async () => {
    loading.value = true;
    try {
      // 格式化日期范围
      let startDate: string | undefined;
      let endDate: string | undefined;

      if (dateRange.value?.[0]) {
        const startValue = dateRange.value[0];
        startDate =
          typeof startValue === 'string'
            ? dayjs(startValue).format('YYYY-MM-DDTHH:mm:ss')
            : dayjs(startValue as any).format('YYYY-MM-DDTHH:mm:ss');
      }

      if (dateRange.value?.[1]) {
        const endValue = dateRange.value[1];
        endDate =
          typeof endValue === 'string'
            ? dayjs(endValue).format('YYYY-MM-DDTHH:mm:ss')
            : dayjs(endValue as any).format('YYYY-MM-DDTHH:mm:ss');
      }

      const params: SystemLogQueryParams = {
        page: pagination.current,
        pageSize: pagination.pageSize,
        level: queryParams.level || undefined,
        keyword: queryParams.keyword || undefined,
        username: queryParams.username || undefined,
        ipAddress: queryParams.ipAddress || undefined,
        requestPath: queryParams.requestPath || undefined,
        startDate,
        endDate,
      };

      const result = await getSystemLogList(params);

      data.value = result.items;
      pagination.total = result.totalCount;
    } catch (error) {
      Message.error(t('common.loadFailed'));
    } finally {
      loading.value = false;
    }
  };

  // 搜索
  const handleSearch = () => {
    pagination.current = 1;
    fetchData();
  };

  // 重置
  const handleReset = () => {
    Object.assign(queryParams, {
      level: '',
      keyword: '',
      username: '',
      ipAddress: '',
      requestPath: '',
      startDate: '',
      endDate: '',
    });
    dateRange.value = [];
    handleSearch();
  };

  // 日期范围变化
  const handleDateRangeChange = () => {
    // 日期范围变化后自动搜索
    handleSearch();
  };

  // 分页变化
  const handlePageChange = (page: number) => {
    pagination.current = page;
    fetchData();
  };

  const handlePageSizeChange = (pageSize: number) => {
    pagination.pageSize = pageSize;
    pagination.current = 1;
    fetchData();
  };

  // 查看详情
  const handleDetail = (record: SystemLog) => {
    currentLog.value = record;
    detailVisible.value = true;
  };

  // 清理日志
  const handleCleanup = () => {
    cleanupForm.beforeDate = dayjs()
      .subtract(30, 'day')
      .format('YYYY-MM-DD HH:mm:ss');
    cleanupVisible.value = true;
  };

  const handleCleanupConfirm = async () => {
    if (!cleanupForm.beforeDate) {
      Message.warning(t('systemLog.selectDate'));
      return;
    }

    try {
      const { data: response } = await cleanupSystemLogs(
        cleanupForm.beforeDate
      );
      if (response.code === 20000) {
        Message.success(response.message);
        cleanupVisible.value = false;
        fetchData();
      }
    } catch (error) {
      Message.error(t('common.operationFailed'));
    }
  };

  // 格式化日期时间
  const formatDateTime = (dateStr: string) => {
    return dayjs(dateStr).format('YYYY-MM-DD HH:mm:ss');
  };

  // 初始化
  onMounted(() => {
    fetchData();
  });
</script>

<style lang="less" scoped>
  .system-log {
    padding: 20px;
  }

  .message-cell {
    max-width: 300px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
</style>

<style lang="less">
  .detail-content {
    .detail-row {
      display: flex;
      padding: 12px 0;
      border-bottom: 1px solid var(--color-border-2);

      &:last-child {
        border-bottom: none;
      }

      .label {
        flex-shrink: 0;
        width: 140px;
        font-weight: 500;
        color: var(--color-text-2);
      }

      .value {
        flex: 1;
        color: var(--color-text-1);
        word-break: break-all;
      }
    }
  }
</style>
