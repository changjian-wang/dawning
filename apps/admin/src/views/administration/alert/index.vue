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
            :title="$t('alert.stats.alertRules')"
            :value="statistics.totalRules"
            :value-style="{ color: '#165DFF' }"
          >
            <template #suffix>
              <span style="font-size: 14px; color: #86909c">
                / {{ statistics.enabledRules }} {{ $t('alert.stats.enabled') }}
              </span>
            </template>
          </a-statistic>
        </a-card>
      </a-col>
      <a-col :xs="24" :sm="12" :md="6">
        <a-card class="stat-card">
          <a-statistic
            :title="$t('alert.stats.todayAlerts')"
            :value="statistics.totalAlertsToday"
            :value-style="{ color: '#F77234' }"
          />
        </a-card>
      </a-col>
      <a-col :xs="24" :sm="12" :md="6">
        <a-card class="stat-card">
          <a-statistic
            :title="$t('alert.stats.unresolvedAlerts')"
            :value="statistics.unresolvedAlerts"
            :value-style="{ color: '#F53F3F' }"
          />
        </a-card>
      </a-col>
      <a-col :xs="24" :sm="12" :md="6">
        <a-card class="stat-card">
          <a-statistic
            :title="$t('alert.stats.criticalAlerts')"
            :value="statistics.criticalAlerts"
            :value-style="{ color: '#EB2F96' }"
          />
        </a-card>
      </a-col>
    </a-row>

    <a-card class="general-card" :title="$t('alert.management')">
      <a-tabs v-model:active-key="activeTab">
        <!-- 告警规则 Tab -->
        <a-tab-pane key="rules" :title="$t('alert.tab.rules')">
          <div class="toolbar">
            <a-button type="primary" size="small" @click="handleAddRule">
              <template #icon><icon-plus /></template>
              {{ $t('alert.addRule') }}
            </a-button>
            <a-button size="small" :loading="checkLoading" @click="handleTriggerCheck">
              <template #icon><icon-thunderbolt /></template>
              {{ $t('alert.manualCheck') }}
            </a-button>
          </div>

          <a-table
            :columns="ruleColumns"
            :data="rules"
            :loading="rulesLoading"
            :pagination="false"
            :bordered="false"
            :stripe="true"
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
            <template #actions="{ record }">
              <a-space>
                <a-button
                  type="text"
                  size="small"
                  @click="handleEditRule(record)"
                >
                  <template #icon><icon-edit /></template>
                  {{ $t('common.edit') }}
                </a-button>
                <a-popconfirm
                  :content="$t('common.deleteConfirm')"
                  @ok="handleDeleteRule(record.id)"
                >
                  <a-button type="text" size="small" status="danger">
                    <template #icon><icon-delete /></template>
                    {{ $t('common.delete') }}
                  </a-button>
                </a-popconfirm>
              </a-space>
            </template>
          </a-table>
        </a-tab-pane>

        <!-- 告警历史 Tab -->
        <a-tab-pane key="history" :title="$t('alert.tab.history')">
          <div class="toolbar">
            <a-space>
              <a-select
                v-model="historyFilter.severity"
                :placeholder="$t('alert.filter.severity')"
                allow-clear
                style="width: 120px"
                @change="loadHistory"
              >
                <a-option
                  v-for="item in localSeverityOptions"
                  :key="item.value"
                  :value="item.value"
                >
                  {{ item.label }}
                </a-option>
              </a-select>
              <a-select
                v-model="historyFilter.status"
                :placeholder="$t('alert.filter.status')"
                allow-clear
                style="width: 120px"
                @change="loadHistory"
              >
                <a-option
                  v-for="item in localAlertStatusOptions"
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
            :bordered="false"
            :stripe="true"
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
                  ({{ $t('alert.column.threshold') }}: {{ record.threshold }})
                </span>
              </span>
            </template>
            <template #triggeredAt="{ record }">
              {{ formatDateTime(record.triggeredAt) }}
            </template>
            <template #actions="{ record }">
              <a-space>
                <a-button
                  v-if="record.status === 'triggered'"
                  type="text"
                  size="small"
                  @click="handleAcknowledge(record)"
                >
                  <template #icon><icon-check /></template>
                  {{ $t('alert.action.acknowledge') }}
                </a-button>
                <a-button
                  v-if="record.status !== 'resolved'"
                  type="text"
                  size="small"
                  status="success"
                  @click="handleResolve(record)"
                >
                  <template #icon><icon-check-circle /></template>
                  {{ $t('alert.action.resolve') }}
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
      :title="isEditMode ? $t('alert.modal.editRule') : $t('alert.modal.addRule')"
      width="600px"
      :ok-loading="ruleSubmitting"
      @before-ok="handleRuleBeforeOk"
      @cancel="handleCancelRule"
    >
      <a-form
        ref="ruleFormRef"
        :model="ruleForm"
        :rules="ruleFormRules"
        layout="vertical"
      >
        <a-form-item field="name" :label="$t('alert.form.ruleName')">
          <a-input v-model="ruleForm.name" :placeholder="$t('alert.placeholder.ruleName')" />
        </a-form-item>
        <a-form-item field="description" :label="$t('alert.form.description')">
          <a-textarea
            v-model="ruleForm.description"
            :placeholder="$t('alert.placeholder.description')"
            :max-length="500"
          />
        </a-form-item>
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item field="metricType" :label="$t('alert.form.metricType')">
              <a-select v-model="ruleForm.metricType" :placeholder="$t('alert.placeholder.metricType')">
                <a-option
                  v-for="item in localMetricTypeOptions"
                  :key="item.value"
                  :value="item.value"
                >
                  {{ item.label }}
                </a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="severity" :label="$t('alert.form.severity')">
              <a-select
                v-model="ruleForm.severity"
                :placeholder="$t('alert.placeholder.severity')"
              >
                <a-option
                  v-for="item in localSeverityOptions"
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
            <a-form-item field="operator" :label="$t('alert.form.operator')">
              <a-select v-model="ruleForm.operator" :placeholder="$t('alert.placeholder.operator')">
                <a-option
                  v-for="item in localOperatorOptions"
                  :key="item.value"
                  :value="item.value"
                >
                  {{ item.label }}
                </a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item field="threshold" :label="$t('alert.form.threshold')">
              <a-input-number
                v-model="ruleForm.threshold"
                :min="0"
                style="width: 100%"
              />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item field="durationSeconds" :label="$t('alert.form.duration')">
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
            <a-form-item field="cooldownMinutes" :label="$t('alert.form.cooldown')">
              <a-input-number
                v-model="ruleForm.cooldownMinutes"
                :min="1"
                style="width: 100%"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="isEnabled" :label="$t('alert.form.enabled')">
              <a-switch v-model="ruleForm.isEnabled" />
            </a-form-item>
          </a-col>
        </a-row>
        <a-divider>{{ $t('alert.form.notifySettings') }}</a-divider>
        <a-form-item field="notifyChannels" :label="$t('alert.form.notifyChannels')">
          <a-checkbox-group v-model="ruleForm.notifyChannels">
            <a-checkbox
              v-for="item in localNotifyChannelOptions"
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
          :label="$t('alert.form.notifyEmails')"
        >
          <a-input
            v-model="ruleForm.notifyEmails"
            :placeholder="$t('alert.placeholder.emails')"
          />
        </a-form-item>
        <a-form-item
          v-if="ruleForm.notifyChannels.includes('webhook')"
          field="webhookUrl"
          :label="$t('alert.form.webhookUrl')"
        >
          <a-input
            v-model="ruleForm.webhookUrl"
            :placeholder="$t('alert.placeholder.webhookUrl')"
          />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted } from 'vue';
  import { Message } from '@arco-design/web-vue';
  import type { TableColumnData } from '@arco-design/web-vue';
  import { useI18n } from 'vue-i18n';
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
    type AlertRuleDto,
    type AlertHistoryDto,
    type AlertStatisticsDto,
    type CreateAlertRuleRequest,
  } from '@/api/alert';

  const { t } = useI18n();

  // State
  const activeTab = ref('rules');
  const rulesLoading = ref(false);
  const historyLoading = ref(false);
  const checkLoading = ref(false);
  const ruleModalVisible = ref(false);
  const ruleSubmitting = ref(false);
  const isEditMode = ref(false);
  const editingRuleId = ref<number | null>(null);

  // Data
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

  // Pagination
  const historyPagination = reactive({
    current: 1,
    pageSize: 10,
    total: 0,
  });

  // Filter
  const historyFilter = reactive({
    severity: undefined as string | undefined,
    status: undefined as string | undefined,
    dateRange: undefined as [string, string] | undefined,
  });

  // Form
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

  const ruleFormRules = computed(() => ({
    name: [{ required: true, message: t('alert.validation.ruleName') }],
    metricType: [{ required: true, message: t('alert.validation.metricType') }],
    operator: [{ required: true, message: t('alert.validation.operator') }],
    threshold: [{ required: true, message: t('alert.validation.threshold') }],
    severity: [{ required: true, message: t('alert.validation.severity') }],
  }));

  // Localized options
  const localMetricTypeOptions = computed(() => [
    { label: t('alert.metricType.cpu'), value: 'cpu' },
    { label: t('alert.metricType.memory'), value: 'memory' },
    { label: t('alert.metricType.responseTime'), value: 'response_time' },
    { label: t('alert.metricType.errorRate'), value: 'error_rate' },
    { label: t('alert.metricType.requestCount'), value: 'request_count' },
  ]);

  const localOperatorOptions = computed(() => [
    { label: t('alert.operator.gt'), value: 'gt' },
    { label: t('alert.operator.gte'), value: 'gte' },
    { label: t('alert.operator.lt'), value: 'lt' },
    { label: t('alert.operator.lte'), value: 'lte' },
    { label: t('alert.operator.eq'), value: 'eq' },
  ]);

  const localSeverityOptions = computed(() => [
    { label: t('alert.severity.info'), value: 'info', color: 'blue' },
    { label: t('alert.severity.warning'), value: 'warning', color: 'orange' },
    { label: t('alert.severity.error'), value: 'error', color: 'red' },
    { label: t('alert.severity.critical'), value: 'critical', color: 'magenta' },
  ]);

  const localAlertStatusOptions = computed(() => [
    { label: t('alert.status.triggered'), value: 'triggered', color: 'red' },
    { label: t('alert.status.acknowledged'), value: 'acknowledged', color: 'orange' },
    { label: t('alert.status.resolved'), value: 'resolved', color: 'green' },
  ]);

  const localNotifyChannelOptions = computed(() => [
    { label: t('alert.notify.email'), value: 'email' },
    { label: t('alert.notify.webhook'), value: 'webhook' },
  ]);

  // Table column definitions
  const ruleColumns = computed<TableColumnData[]>(() => [
    { title: t('alert.column.ruleName'), dataIndex: 'name', ellipsis: true, width: 150 },
    { title: t('alert.column.metricType'), slotName: 'metricType', width: 120 },
    { title: t('alert.column.condition'), slotName: 'condition', width: 100 },
    {
      title: t('alert.column.duration'),
      dataIndex: 'durationSeconds',
      width: 90,
      render: ({ record }: { record: AlertRuleDto }) =>
        `${record.durationSeconds}${t('alert.duration.seconds')}`,
    },
    { title: t('alert.column.severity'), slotName: 'severity', width: 100 },
    { title: t('alert.column.enabled'), slotName: 'isEnabled', width: 80 },
    { title: t('alert.column.lastTriggered'), slotName: 'lastTriggeredAt', width: 160 },
    { title: t('common.actions'), slotName: 'actions', width: 160 },
  ]);

  const historyColumns = computed<TableColumnData[]>(() => [
    { title: t('alert.column.ruleName'), dataIndex: 'ruleName', ellipsis: true, width: 150 },
    { title: t('alert.column.severity'), slotName: 'severity', width: 90 },
    { title: t('alert.column.status'), slotName: 'status', width: 90 },
    { title: t('alert.column.metricValue'), slotName: 'metricValue', width: 150 },
    { title: t('alert.column.message'), dataIndex: 'message', ellipsis: true },
    { title: t('alert.column.triggeredAt'), slotName: 'triggeredAt', width: 160 },
    { title: t('common.actions'), slotName: 'actions', width: 160 },
  ]);

  // Utility functions
  const formatDateTime = (dateStr: string) => {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleString('zh-CN');
  };

  const getMetricTypeLabel = (type: string) => {
    return localMetricTypeOptions.value.find((o) => o.value === type)?.label || type;
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
    return localSeverityOptions.value.find((o) => o.value === severity)?.label || severity;
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
    return localAlertStatusOptions.value.find((o) => o.value === status)?.label || status;
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      triggered: 'red',
      acknowledged: 'orange',
      resolved: 'green',
    };
    return colors[status] || 'gray';
  };

  // Load data
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
        const [startTime, endTime] = historyFilter.dateRange;
        params.startTime = startTime;
        params.endTime = endTime;
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
    } catch {
      // Loading statistics failed, does not affect main flow
    }
  };

  // Event handlers
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

  // Initialize
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
