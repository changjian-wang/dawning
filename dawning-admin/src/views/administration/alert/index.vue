<template>
  <div class="container">
    <Breadcrumb
      :items="['menu.administration', 'menu.administration.alertManagement']"
    />

    <!-- 统计卡片 -->
    <a-row :gutter="16" style="margin-bottom: 16px">
      <a-col :xs="24" :sm="12" :md="6">
        <a-card class="stat-card">
          <a-statistic
            title="告警规则"
            :value="statistics.totalRules"
            :value-style="{ color: '#165DFF' }"
          >
            <template #suffix>
              <span style="font-size: 14px; color: #86909c">
                / {{ statistics.enabledRules }} 启用
              </span>
            </template>
          </a-statistic>
        </a-card>
      </a-col>
      <a-col :xs="24" :sm="12" :md="6">
        <a-card class="stat-card">
          <a-statistic
            title="今日告警"
            :value="statistics.totalAlertsToday"
            :value-style="{ color: '#F77234' }"
          />
        </a-card>
      </a-col>
      <a-col :xs="24" :sm="12" :md="6">
        <a-card class="stat-card">
          <a-statistic
            title="未解决告警"
            :value="statistics.unresolvedAlerts"
            :value-style="{ color: '#F53F3F' }"
          />
        </a-card>
      </a-col>
      <a-col :xs="24" :sm="12" :md="6">
        <a-card class="stat-card">
          <a-statistic
            title="严重告警"
            :value="statistics.criticalAlerts"
            :value-style="{ color: '#EB2F96' }"
          />
        </a-card>
      </a-col>
    </a-row>

    <a-card class="general-card" title="告警管理">
      <a-tabs v-model:active-key="activeTab">
        <!-- 告警规则 Tab -->
        <a-tab-pane key="rules" title="告警规则">
          <div class="toolbar">
            <a-button type="primary" @click="handleAddRule">
              <template #icon><icon-plus /></template>
              添加规则
            </a-button>
            <a-button @click="handleTriggerCheck" :loading="checkLoading">
              <template #icon><icon-thunderbolt /></template>
              手动检查
            </a-button>
          </div>

          <a-table
            :columns="ruleColumns"
            :data="rules"
            :loading="rulesLoading"
            :pagination="false"
            row-key="id"
          >
            <template #metricType="{ record }">
              <a-tag>{{ getMetricTypeLabel(record.metricType) }}</a-tag>
            </template>
            <template #condition="{ record }">
              <span>
                {{ getOperatorLabel(record.operator) }} {{ record.threshold }}
              </span>
            </template>
            <template #severity="{ record }">
              <a-tag :color="getSeverityColor(record.severity)">
                {{ getSeverityLabel(record.severity) }}
              </a-tag>
            </template>
            <template #isEnabled="{ record }">
              <a-switch
                :model-value="record.isEnabled"
                @change="(val) => handleToggleRule(record, val as boolean)"
              />
            </template>
            <template #lastTriggeredAt="{ record }">
              <span v-if="record.lastTriggeredAt">
                {{ formatDateTime(record.lastTriggeredAt) }}
              </span>
              <span v-else style="color: #86909c">-</span>
            </template>
            <template #operations="{ record }">
              <a-space>
                <a-button
                  type="text"
                  size="small"
                  @click="handleEditRule(record)"
                >
                  <template #icon><icon-edit /></template>
                </a-button>
                <a-popconfirm
                  content="确定要删除此告警规则吗？"
                  @ok="handleDeleteRule(record.id)"
                >
                  <a-button type="text" size="small" status="danger">
                    <template #icon><icon-delete /></template>
                  </a-button>
                </a-popconfirm>
              </a-space>
            </template>
          </a-table>
        </a-tab-pane>

        <!-- 告警历史 Tab -->
        <a-tab-pane key="history" title="告警历史">
          <div class="toolbar">
            <a-space>
              <a-select
                v-model="historyFilter.severity"
                placeholder="严重程度"
                allow-clear
                style="width: 120px"
                @change="loadHistory"
              >
                <a-option
                  v-for="item in severityOptions"
                  :key="item.value"
                  :value="item.value"
                >
                  {{ item.label }}
                </a-option>
              </a-select>
              <a-select
                v-model="historyFilter.status"
                placeholder="状态"
                allow-clear
                style="width: 120px"
                @change="loadHistory"
              >
                <a-option
                  v-for="item in alertStatusOptions"
                  :key="item.value"
                  :value="item.value"
                >
                  {{ item.label }}
                </a-option>
              </a-select>
              <a-range-picker
                v-model="historyFilter.dateRange"
                style="width: 280px"
                @change="loadHistory"
              />
            </a-space>
          </div>

          <a-table
            :columns="historyColumns"
            :data="historyList"
            :loading="historyLoading"
            :pagination="historyPagination"
            row-key="id"
            @page-change="handleHistoryPageChange"
          >
            <template #severity="{ record }">
              <a-tag :color="getSeverityColor(record.severity)">
                {{ getSeverityLabel(record.severity) }}
              </a-tag>
            </template>
            <template #status="{ record }">
              <a-tag :color="getStatusColor(record.status)">
                {{ getStatusLabel(record.status) }}
              </a-tag>
            </template>
            <template #metricValue="{ record }">
              <span>
                {{ record.metricValue.toFixed(2) }}
                <span style="color: #86909c; font-size: 12px">
                  (阈值: {{ record.threshold }})
                </span>
              </span>
            </template>
            <template #triggeredAt="{ record }">
              {{ formatDateTime(record.triggeredAt) }}
            </template>
            <template #operations="{ record }">
              <a-space>
                <a-button
                  v-if="record.status === 'triggered'"
                  type="text"
                  size="small"
                  @click="handleAcknowledge(record)"
                >
                  确认
                </a-button>
                <a-button
                  v-if="record.status !== 'resolved'"
                  type="text"
                  size="small"
                  status="success"
                  @click="handleResolve(record)"
                >
                  解决
                </a-button>
              </a-space>
            </template>
          </a-table>
        </a-tab-pane>
      </a-tabs>
    </a-card>

    <!-- 规则编辑弹窗 -->
    <a-modal
      v-model:visible="ruleModalVisible"
      :title="isEditMode ? '编辑告警规则' : '添加告警规则'"
      :ok-loading="ruleSubmitting"
      @before-ok="handleRuleBeforeOk"
      @cancel="handleCancelRule"
      width="600px"
    >
      <a-form
        ref="ruleFormRef"
        :model="ruleForm"
        :rules="ruleFormRules"
        layout="vertical"
      >
        <a-form-item field="name" label="规则名称">
          <a-input v-model="ruleForm.name" placeholder="请输入规则名称" />
        </a-form-item>
        <a-form-item field="description" label="描述">
          <a-textarea
            v-model="ruleForm.description"
            placeholder="请输入规则描述"
            :max-length="500"
          />
        </a-form-item>
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item field="metricType" label="指标类型">
              <a-select v-model="ruleForm.metricType" placeholder="请选择指标">
                <a-option
                  v-for="item in metricTypeOptions"
                  :key="item.value"
                  :value="item.value"
                >
                  {{ item.label }}
                </a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="severity" label="严重程度">
              <a-select v-model="ruleForm.severity" placeholder="请选择严重程度">
                <a-option
                  v-for="item in severityOptions"
                  :key="item.value"
                  :value="item.value"
                >
                  {{ item.label }}
                </a-option>
              </a-select>
            </a-form-item>
          </a-col>
        </a-row>
        <a-row :gutter="16">
          <a-col :span="8">
            <a-form-item field="operator" label="比较操作符">
              <a-select v-model="ruleForm.operator" placeholder="请选择操作符">
                <a-option
                  v-for="item in operatorOptions"
                  :key="item.value"
                  :value="item.value"
                >
                  {{ item.label }}
                </a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item field="threshold" label="阈值">
              <a-input-number
                v-model="ruleForm.threshold"
                :min="0"
                style="width: 100%"
              />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item field="durationSeconds" label="持续时间(秒)">
              <a-input-number
                v-model="ruleForm.durationSeconds"
                :min="0"
                style="width: 100%"
              />
            </a-form-item>
          </a-col>
        </a-row>
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item field="cooldownMinutes" label="冷却时间(分钟)">
              <a-input-number
                v-model="ruleForm.cooldownMinutes"
                :min="1"
                style="width: 100%"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="isEnabled" label="启用状态">
              <a-switch v-model="ruleForm.isEnabled" />
            </a-form-item>
          </a-col>
        </a-row>
        <a-divider>通知设置</a-divider>
        <a-form-item field="notifyChannels" label="通知渠道">
          <a-checkbox-group v-model="ruleForm.notifyChannels">
            <a-checkbox
              v-for="item in notifyChannelOptions"
              :key="item.value"
              :value="item.value"
            >
              {{ item.label }}
            </a-checkbox>
          </a-checkbox-group>
        </a-form-item>
        <a-form-item
          v-if="ruleForm.notifyChannels.includes('email')"
          field="notifyEmails"
          label="通知邮箱"
        >
          <a-input
            v-model="ruleForm.notifyEmails"
            placeholder="多个邮箱用逗号分隔"
          />
        </a-form-item>
        <a-form-item
          v-if="ruleForm.notifyChannels.includes('webhook')"
          field="webhookUrl"
          label="Webhook URL"
        >
          <a-input v-model="ruleForm.webhookUrl" placeholder="请输入 Webhook URL" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted, computed } from 'vue';
import { Message } from '@arco-design/web-vue';
import type { TableColumnData } from '@arco-design/web-vue';
import Breadcrumb from '@/components/breadcrumb/index.vue';
import {
  getAllRules,
  createRule,
  updateRule,
  deleteRule,
  setRuleEnabled,
  getAlertHistory,
  updateAlertStatus,
  triggerAlertCheck,
  getAlertStatistics,
  metricTypeOptions,
  operatorOptions,
  severityOptions,
  alertStatusOptions,
  notifyChannelOptions,
  type AlertRuleDto,
  type AlertHistoryDto,
  type AlertStatisticsDto,
  type CreateAlertRuleRequest,
} from '@/api/alert';

// 状态
const activeTab = ref('rules');
const rulesLoading = ref(false);
const historyLoading = ref(false);
const checkLoading = ref(false);
const ruleModalVisible = ref(false);
const ruleSubmitting = ref(false);
const isEditMode = ref(false);
const editingRuleId = ref<number | null>(null);

// 数据
const rules = ref<AlertRuleDto[]>([]);
const historyList = ref<AlertHistoryDto[]>([]);
const statistics = ref<AlertStatisticsDto>({
  totalRules: 0,
  enabledRules: 0,
  totalAlertsToday: 0,
  unresolvedAlerts: 0,
  criticalAlerts: 0,
  warningAlerts: 0,
  alertsByMetricType: {},
  alertsBySeverity: {},
});

// 分页
const historyPagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
});

// 过滤
const historyFilter = reactive({
  severity: undefined as string | undefined,
  status: undefined as string | undefined,
  dateRange: undefined as [string, string] | undefined,
});

// 表单
const ruleFormRef = ref();
const ruleForm = reactive<CreateAlertRuleRequest>({
  name: '',
  description: '',
  metricType: 'cpu',
  operator: 'gt',
  threshold: 80,
  durationSeconds: 60,
  severity: 'warning',
  isEnabled: true,
  notifyChannels: [],
  notifyEmails: '',
  webhookUrl: '',
  cooldownMinutes: 5,
});

const ruleFormRules = {
  name: [{ required: true, message: '请输入规则名称' }],
  metricType: [{ required: true, message: '请选择指标类型' }],
  operator: [{ required: true, message: '请选择操作符' }],
  threshold: [{ required: true, message: '请输入阈值' }],
  severity: [{ required: true, message: '请选择严重程度' }],
};

// 表格列定义
const ruleColumns: TableColumnData[] = [
  { title: '规则名称', dataIndex: 'name', ellipsis: true, width: 150 },
  { title: '指标类型', slotName: 'metricType', width: 120 },
  { title: '条件', slotName: 'condition', width: 100 },
  { title: '持续时间', dataIndex: 'durationSeconds', width: 90, render: ({ record }: { record: AlertRuleDto }) => `${record.durationSeconds}秒` },
  { title: '严重程度', slotName: 'severity', width: 100 },
  { title: '启用', slotName: 'isEnabled', width: 80 },
  { title: '上次触发', slotName: 'lastTriggeredAt', width: 160 },
  { title: '操作', slotName: 'operations', width: 100 },
];

const historyColumns: TableColumnData[] = [
  { title: '规则名称', dataIndex: 'ruleName', ellipsis: true, width: 150 },
  { title: '严重程度', slotName: 'severity', width: 90 },
  { title: '状态', slotName: 'status', width: 90 },
  { title: '指标值', slotName: 'metricValue', width: 150 },
  { title: '消息', dataIndex: 'message', ellipsis: true },
  { title: '触发时间', slotName: 'triggeredAt', width: 160 },
  { title: '操作', slotName: 'operations', width: 120 },
];

// 工具函数
const formatDateTime = (dateStr: string) => {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleString('zh-CN');
};

const getMetricTypeLabel = (type: string) => {
  return metricTypeOptions.find((o) => o.value === type)?.label || type;
};

const getOperatorLabel = (op: string) => {
  const labels: Record<string, string> = {
    gt: '>',
    gte: '>=',
    lt: '<',
    lte: '<=',
    eq: '=',
  };
  return labels[op] || op;
};

const getSeverityLabel = (severity: string) => {
  return severityOptions.find((o) => o.value === severity)?.label || severity;
};

const getSeverityColor = (severity: string) => {
  const colors: Record<string, string> = {
    info: 'blue',
    warning: 'orange',
    error: 'red',
    critical: 'magenta',
  };
  return colors[severity] || 'gray';
};

const getStatusLabel = (status: string) => {
  return alertStatusOptions.find((o) => o.value === status)?.label || status;
};

const getStatusColor = (status: string) => {
  const colors: Record<string, string> = {
    triggered: 'red',
    acknowledged: 'orange',
    resolved: 'green',
  };
  return colors[status] || 'gray';
};

// 加载数据
const loadRules = async () => {
  rulesLoading.value = true;
  try {
    const { data } = await getAllRules();
    rules.value = data;
  } catch (error) {
    Message.error('加载告警规则失败');
  } finally {
    rulesLoading.value = false;
  }
};

const loadHistory = async () => {
  historyLoading.value = true;
  try {
    const params: any = {
      page: historyPagination.current,
      pageSize: historyPagination.pageSize,
    };
    if (historyFilter.severity) params.severity = historyFilter.severity;
    if (historyFilter.status) params.status = historyFilter.status;
    if (historyFilter.dateRange) {
      params.startTime = historyFilter.dateRange[0];
      params.endTime = historyFilter.dateRange[1];
    }

    const { data } = await getAlertHistory(params);
    historyList.value = data.items;
    historyPagination.total = data.totalCount;
  } catch (error) {
    Message.error('加载告警历史失败');
  } finally {
    historyLoading.value = false;
  }
};

const loadStatistics = async () => {
  try {
    const { data } = await getAlertStatistics();
    statistics.value = data;
  } catch (error) {
    console.error('加载统计数据失败', error);
  }
};

// 事件处理
const handleAddRule = () => {
  isEditMode.value = false;
  editingRuleId.value = null;
  Object.assign(ruleForm, {
    name: '',
    description: '',
    metricType: 'cpu',
    operator: 'gt',
    threshold: 80,
    durationSeconds: 60,
    severity: 'warning',
    isEnabled: true,
    notifyChannels: [],
    notifyEmails: '',
    webhookUrl: '',
    cooldownMinutes: 5,
  });
  ruleModalVisible.value = true;
};

const handleEditRule = (record: AlertRuleDto) => {
  isEditMode.value = true;
  editingRuleId.value = record.id;
  Object.assign(ruleForm, {
    name: record.name,
    description: record.description,
    metricType: record.metricType,
    operator: record.operator,
    threshold: record.threshold,
    durationSeconds: record.durationSeconds,
    severity: record.severity,
    isEnabled: record.isEnabled,
    notifyChannels: record.notifyChannels || [],
    notifyEmails: record.notifyEmails,
    webhookUrl: record.webhookUrl,
    cooldownMinutes: record.cooldownMinutes,
  });
  ruleModalVisible.value = true;
};

const handleRuleBeforeOk = async (done: (closed: boolean) => void) => {
  const errors = await ruleFormRef.value?.validate();
  if (errors) {
    done(false);
    return;
  }

  ruleSubmitting.value = true;
  try {
    if (isEditMode.value && editingRuleId.value) {
      await updateRule(editingRuleId.value, ruleForm);
      Message.success('更新成功');
    } else {
      await createRule(ruleForm);
      Message.success('创建成功');
    }
    done(true);
    loadRules();
    loadStatistics();
  } catch (error) {
    Message.error(isEditMode.value ? '更新失败' : '创建失败');
    done(false);
  } finally {
    ruleSubmitting.value = false;
  }
};

const handleCancelRule = () => {
  ruleModalVisible.value = false;
};

const handleDeleteRule = async (id: number) => {
  try {
    await deleteRule(id);
    Message.success('删除成功');
    loadRules();
    loadStatistics();
  } catch (error) {
    Message.error('删除失败');
  }
};

const handleToggleRule = async (record: AlertRuleDto, enabled: boolean) => {
  try {
    await setRuleEnabled(record.id, enabled);
    Message.success(enabled ? '已启用' : '已禁用');
    loadRules();
    loadStatistics();
  } catch (error) {
    Message.error('操作失败');
  }
};

const handleTriggerCheck = async () => {
  checkLoading.value = true;
  try {
    const { data } = await triggerAlertCheck();
    Message.success(
      `检查完成: 检查了 ${data.rulesChecked} 条规则, 触发了 ${data.alertsTriggered} 条告警`
    );
    loadHistory();
    loadStatistics();
  } catch (error) {
    Message.error('告警检查失败');
  } finally {
    checkLoading.value = false;
  }
};

const handleAcknowledge = async (record: AlertHistoryDto) => {
  try {
    await updateAlertStatus(record.id, { status: 'acknowledged' });
    Message.success('已确认');
    loadHistory();
    loadStatistics();
  } catch (error) {
    Message.error('操作失败');
  }
};

const handleResolve = async (record: AlertHistoryDto) => {
  try {
    await updateAlertStatus(record.id, { status: 'resolved' });
    Message.success('已解决');
    loadHistory();
    loadStatistics();
  } catch (error) {
    Message.error('操作失败');
  }
};

const handleHistoryPageChange = (page: number) => {
  historyPagination.current = page;
  loadHistory();
};

// 初始化
onMounted(() => {
  loadRules();
  loadHistory();
  loadStatistics();
});
</script>

<style scoped lang="less">
.container {
  padding: 20px;
}

.stat-card {
  margin-bottom: 16px;
}

.toolbar {
  display: flex;
  justify-content: space-between;
  margin-bottom: 16px;
  gap: 12px;
}

.general-card {
  margin-bottom: 16px;
}
</style>
