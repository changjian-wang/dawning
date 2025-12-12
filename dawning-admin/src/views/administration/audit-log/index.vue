<template>
  <div class="container">
    <Breadcrumb :items="['menu.administration', 'menu.administration.auditLog']" />
    
    <a-card class="general-card" :title="$t('auditLog.list.title')">
      <!-- 搜索表单 -->
      <a-row>
        <a-col :flex="1">
          <a-form
            :model="searchForm"
            :label-col-props="{ span: 6 }"
            :wrapper-col-props="{ span: 18 }"
            label-align="left"
          >
            <a-row :gutter="16">
              <a-col :span="8">
                <a-form-item field="username" :label="$t('auditLog.form.username')">
                  <a-input
                    v-model="searchForm.username"
                    :placeholder="$t('auditLog.form.username.placeholder')"
                  />
                </a-form-item>
              </a-col>
              
              <a-col :span="8">
                <a-form-item field="action" :label="$t('auditLog.form.action')">
                  <a-select
                    v-model="searchForm.action"
                    :placeholder="$t('auditLog.form.action.placeholder')"
                    allow-clear
                  >
                    <a-option value="Create">{{ $t('auditLog.action.Create') }}</a-option>
                    <a-option value="Update">{{ $t('auditLog.action.Update') }}</a-option>
                    <a-option value="Delete">{{ $t('auditLog.action.Delete') }}</a-option>
                    <a-option value="ChangePassword">{{ $t('auditLog.action.ChangePassword') }}</a-option>
                    <a-option value="ResetPassword">{{ $t('auditLog.action.ResetPassword') }}</a-option>
                  </a-select>
                </a-form-item>
              </a-col>
              
              <a-col :span="8">
                <a-form-item field="entityType" :label="$t('auditLog.form.entityType')">
                  <a-select
                    v-model="searchForm.entityType"
                    :placeholder="$t('auditLog.form.entityType.placeholder')"
                    allow-clear
                  >
                    <a-option value="User">{{ $t('auditLog.entityType.User') }}</a-option>
                    <a-option value="Role">{{ $t('auditLog.entityType.Role') }}</a-option>
                    <a-option value="Application">{{ $t('auditLog.entityType.Application') }}</a-option>
                    <a-option value="Scope">{{ $t('auditLog.entityType.Scope') }}</a-option>
                  </a-select>
                </a-form-item>
              </a-col>
            </a-row>
            
            <a-row :gutter="16">
              <a-col :span="8">
                <a-form-item field="ipAddress" :label="$t('auditLog.form.ipAddress')">
                  <a-input
                    v-model="searchForm.ipAddress"
                    :placeholder="$t('auditLog.form.ipAddress.placeholder')"
                  />
                </a-form-item>
              </a-col>
              
              <a-col :span="8">
                <a-form-item field="dateRange" :label="$t('auditLog.form.dateRange')">
                  <a-range-picker
                    v-model="searchForm.dateRange"
                    style="width: 100%"
                    show-time
                    format="YYYY-MM-DD HH:mm:ss"
                  />
                </a-form-item>
              </a-col>
              
              <a-col :span="8">
                <a-form-item>
                  <a-space>
                    <a-button type="primary" @click="handleSearch">
                      <template #icon><icon-search /></template>
                      {{ $t('auditLog.form.search') }}
                    </a-button>
                    <a-button @click="handleReset">
                      <template #icon><icon-refresh /></template>
                      {{ $t('auditLog.form.reset') }}
                    </a-button>
                  </a-space>
                </a-form-item>
              </a-col>
            </a-row>
          </a-form>
        </a-col>
      </a-row>
      
      <!-- 数据表格 -->
      <a-table
        row-key="id"
        :loading="loading"
        :columns="columns"
        :data="tableData"
        :pagination="pagination"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      >
        <template #action="{ record }">
          <a-tag :color="getActionColor(record.action)">
            {{ $t(`auditLog.action.${record.action}`) }}
          </a-tag>
        </template>
        
        <template #entityType="{ record }">
          <span v-if="record.entityType">
            {{ $t(`auditLog.entityType.${record.entityType}`) }}
          </span>
          <span v-else>-</span>
        </template>
        
        <template #statusCode="{ record }">
          <a-tag v-if="record.statusCode" :color="getStatusColor(record.statusCode)">
            {{ record.statusCode }}
          </a-tag>
          <span v-else>-</span>
        </template>
        
        <template #createdAt="{ record }">
          {{ formatDateTime(record.createdAt) }}
        </template>
        
        <template #operations="{ record }">
          <a-button type="text" size="small" @click="handleViewDetail(record)">
            {{ $t('auditLog.button.view') }}
          </a-button>
        </template>
      </a-table>
    </a-card>
    
    <!-- 详情对话框 -->
    <a-modal
      v-model:visible="detailVisible"
      :title="$t('auditLog.detail.title')"
      width="650px"
      :footer="false"
    >
      <div v-if="currentLog" class="detail-content">
        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.username') }}</span>
          <span class="value">{{ currentLog.username || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.action') }}</span>
          <span class="value">
            <a-tag :color="getActionColor(currentLog.action)" size="small">
              {{ $t(`auditLog.action.${currentLog.action}`) }}
            </a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.entityType') }}</span>
          <span class="value">{{ currentLog.entityType ? $t(`auditLog.entityType.${currentLog.entityType}`) : '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.entityId') }}</span>
          <span class="value">{{ currentLog.entityId || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.description') }}</span>
          <span class="value">{{ currentLog.description || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.ipAddress') }}</span>
          <span class="value">{{ currentLog.ipAddress || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.statusCode') }}</span>
          <span class="value">
            <a-tag v-if="currentLog.statusCode" :color="getStatusColor(currentLog.statusCode)" size="small">
              {{ currentLog.statusCode }}
            </a-tag>
            <span v-else>-</span>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.requestMethod') }}</span>
          <span class="value">{{ currentLog.requestMethod || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.requestPath') }}</span>
          <span class="value">{{ currentLog.requestPath || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.userAgent') }}</span>
          <span class="value" style="word-break: break-all">{{ currentLog.userAgent || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('auditLog.detail.createdAt') }}</span>
          <span class="value">{{ formatDateTime(currentLog.createdAt) }}</span>
        </div>
        
        <!-- 变更内容 -->
        <div v-if="currentLog.oldValues || currentLog.newValues" style="margin-top: 20px; padding-top: 20px; border-top: 1px solid var(--color-border-2)">
          <h3 style="margin-bottom: 16px; font-size: 15px; font-weight: 600">{{ $t('auditLog.detail.changes') }}</h3>
          <a-row :gutter="16">
            <a-col :span="12">
              <div class="change-section">
                <h4>{{ $t('auditLog.detail.oldValues') }}</h4>
                <pre v-if="currentLog.oldValues" class="json-display">{{ formatJson(currentLog.oldValues) }}</pre>
                <a-empty v-else :description="$t('auditLog.detail.noChanges')" />
              </div>
            </a-col>
            <a-col :span="12">
              <div class="change-section">
                <h4>{{ $t('auditLog.detail.newValues') }}</h4>
                <pre v-if="currentLog.newValues" class="json-display">{{ formatJson(currentLog.newValues) }}</pre>
                <a-empty v-else :description="$t('auditLog.detail.noChanges')" />
              </div>
            </a-col>
          </a-row>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { Message } from '@arco-design/web-vue';
import type { TableColumnData } from '@arco-design/web-vue/es/table/interface';
import { getAuditLogs, type AuditLog, type AuditLogQueryParams } from '@/api/audit-log';
import dayjs from 'dayjs';

const { t } = useI18n();

// 搜索表单（包含前端专用的dateRange字段）
interface SearchFormData extends AuditLogQueryParams {
  dateRange?: [string, string];
}

const searchForm = reactive<SearchFormData>({
  username: '',
  action: undefined,
  entityType: undefined,
  ipAddress: '',
  dateRange: undefined,
});

// 表格数据
const loading = ref(false);
const tableData = ref<AuditLog[]>([]);
const pagination = reactive({
  current: 1,
  pageSize: 20,
  total: 0,
  showTotal: true,
  showPageSize: true,
});

// 详情对话框
const detailVisible = ref(false);
const currentLog = ref<AuditLog | null>(null);

// 表格列定义
const columns = computed<TableColumnData[]>(() => [
  {
    title: t('auditLog.columns.username'),
    dataIndex: 'username',
    width: 120,
  },
  {
    title: t('auditLog.columns.action'),
    dataIndex: 'action',
    slotName: 'action',
    width: 120,
  },
  {
    title: t('auditLog.columns.entityType'),
    dataIndex: 'entityType',
    slotName: 'entityType',
    width: 120,
  },
  {
    title: t('auditLog.columns.description'),
    dataIndex: 'description',
    ellipsis: true,
    tooltip: true,
  },
  {
    title: t('auditLog.columns.ipAddress'),
    dataIndex: 'ipAddress',
    width: 140,
  },
  {
    title: t('auditLog.columns.statusCode'),
    dataIndex: 'statusCode',
    slotName: 'statusCode',
    width: 100,
  },
  {
    title: t('auditLog.columns.createdAt'),
    dataIndex: 'createdAt',
    slotName: 'createdAt',
    width: 180,
  },
  {
    title: t('auditLog.columns.operations'),
    slotName: 'operations',
    width: 100,
    fixed: 'right',
  },
]);

// 加载数据
const fetchData = async () => {
  loading.value = true;
  try {
    const params: AuditLogQueryParams = {
      page: pagination.current,
      pageSize: pagination.pageSize,
      username: searchForm.username || undefined,
      action: searchForm.action,
      entityType: searchForm.entityType,
      ipAddress: searchForm.ipAddress || undefined,
      startDate: searchForm.dateRange?.[0] 
        ? dayjs(searchForm.dateRange[0]).format('YYYY-MM-DDTHH:mm:ss')
        : undefined,
      endDate: searchForm.dateRange?.[1]
        ? dayjs(searchForm.dateRange[1]).format('YYYY-MM-DDTHH:mm:ss')
        : undefined,
    };
    
    const result = await getAuditLogs(params);
    
    tableData.value = result.items;
    pagination.total = result.totalCount;
    pagination.current = result.pageIndex;
    pagination.pageSize = result.pageSize;
  } catch (error) {
    Message.error(t('auditLog.message.error'));
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
  searchForm.username = '';
  searchForm.action = undefined;
  searchForm.entityType = undefined;
  searchForm.ipAddress = '';
  searchForm.dateRange = undefined;
  pagination.current = 1;
  fetchData();
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
const handleViewDetail = (record: AuditLog) => {
  currentLog.value = record;
  detailVisible.value = true;
};

// 辅助函数
const getActionColor = (action: string) => {
  const colorMap: Record<string, string> = {
    Create: 'green',
    Update: 'blue',
    Delete: 'red',
    ChangePassword: 'orange',
    ResetPassword: 'purple',
  };
  return colorMap[action] || 'gray';
};

const getStatusColor = (statusCode: number) => {
  if (statusCode >= 200 && statusCode < 300) return 'green';
  if (statusCode >= 400 && statusCode < 500) return 'orange';
  if (statusCode >= 500) return 'red';
  return 'gray';
};

const formatDateTime = (dateTime: string) => {
  return dayjs(dateTime).format('YYYY-MM-DD HH:mm:ss');
};

const formatJson = (obj: any) => {
  if (typeof obj === 'string') {
    try {
      obj = JSON.parse(obj);
    } catch {
      return obj;
    }
  }
  return JSON.stringify(obj, null, 2);
};

onMounted(() => {
  fetchData();
});
</script>

<style scoped lang="less">
.container {
  padding: 0 20px 20px 20px;
}

// 极简列表风格
.detail-content {
  .detail-row {
    display: flex;
    padding: 14px 0;
    border-bottom: 1px solid var(--color-border-1);
    align-items: flex-start;

    &:last-child {
      border-bottom: none;
    }

    .label {
      width: 110px;
      flex-shrink: 0;
      font-size: 14px;
      color: var(--color-text-3);
      line-height: 1.5;
    }

    .value {
      flex: 1;
      font-size: 14px;
      color: var(--color-text-1);
      line-height: 1.5;
      word-break: break-word;
    }
  }

  h3 {
    margin-bottom: 16px;
    font-size: 16px;
    font-weight: 500;
  }
  
  h4 {
    margin-bottom: 8px;
    font-size: 14px;
    font-weight: 500;
  }
}

.change-section {
  padding: 12px;
  background-color: var(--color-fill-2);
  border-radius: 4px;
  
  .json-display {
    margin: 0;
    padding: 12px;
    background-color: var(--color-bg-1);
    border-radius: 4px;
    font-size: 12px;
    line-height: 1.5;
    max-height: 300px;
    overflow: auto;
  }
}
</style>
