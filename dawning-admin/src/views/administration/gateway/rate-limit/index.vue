<template>
  <div class="container">
    <a-card class="general-card" :title="$t('rateLimit.title')">
      <a-tabs default-active-key="policies">
        <!-- 限流策略 Tab -->
        <a-tab-pane key="policies" :title="$t('rateLimit.policies')">
          <div class="toolbar">
            <a-button type="primary" @click="handleAddPolicy">
              <template #icon><icon-plus /></template>
              {{ $t('rateLimit.addPolicy') }}
            </a-button>
          </div>

          <a-table
            :columns="policyColumns"
            :data="policies"
            :loading="policyLoading"
            :pagination="false"
            row-key="id"
          >
            <template #policyType="{ record }">
              <a-tag :color="getPolicyTypeColor(record.policyType)">
                {{ getPolicyTypeLabel(record.policyType) }}
              </a-tag>
            </template>
            <template #permitLimit="{ record }">
              <span>{{ record.permitLimit }} / {{ record.windowSeconds }}s</span>
            </template>
            <template #isEnabled="{ record }">
              <a-switch
                :model-value="record.isEnabled"
                @change="(val) => handleTogglePolicy(record, val as boolean)"
              />
            </template>
            <template #operations="{ record }">
              <a-space>
                <a-button type="text" size="small" @click="handleEditPolicy(record)">
                  <template #icon><icon-edit /></template>
                </a-button>
                <a-popconfirm
                  :content="$t('rateLimit.deleteConfirm')"
                  @ok="handleDeletePolicy(record.id)"
                >
                  <a-button type="text" size="small" status="danger">
                    <template #icon><icon-delete /></template>
                  </a-button>
                </a-popconfirm>
              </a-space>
            </template>
          </a-table>
        </a-tab-pane>

        <!-- IP 黑白名单 Tab -->
        <a-tab-pane key="ip-rules" :title="$t('rateLimit.ipRules')">
          <div class="toolbar">
            <a-space>
              <a-select
                v-model="ipFilter.ruleType"
                :placeholder="$t('rateLimit.ruleType')"
                allow-clear
                style="width: 120px"
                @change="loadIpRules"
              >
                <a-option value="blacklist">{{ $t('rateLimit.blacklist') }}</a-option>
                <a-option value="whitelist">{{ $t('rateLimit.whitelist') }}</a-option>
              </a-select>
              <a-button type="primary" @click="handleAddIpRule">
                <template #icon><icon-plus /></template>
                {{ $t('rateLimit.addIpRule') }}
              </a-button>
            </a-space>
          </div>

          <a-table
            :columns="ipRuleColumns"
            :data="ipRules"
            :loading="ipLoading"
            :pagination="ipPagination"
            row-key="id"
            @page-change="handleIpPageChange"
          >
            <template #ruleType="{ record }">
              <a-tag :color="record.ruleType === 'blacklist' ? 'red' : 'green'">
                {{ record.ruleType === 'blacklist' ? $t('rateLimit.blacklist') : $t('rateLimit.whitelist') }}
              </a-tag>
            </template>
            <template #isEnabled="{ record }">
              <a-switch
                :model-value="record.isEnabled"
                @change="(val) => handleToggleIpRule(record, val as boolean)"
              />
            </template>
            <template #expiresAt="{ record }">
              <span v-if="record.expiresAt">{{ formatDate(record.expiresAt) }}</span>
              <span v-else class="text-secondary">{{ $t('rateLimit.permanent') }}</span>
            </template>
            <template #operations="{ record }">
              <a-space>
                <a-button type="text" size="small" @click="handleEditIpRule(record)">
                  <template #icon><icon-edit /></template>
                </a-button>
                <a-popconfirm
                  :content="$t('rateLimit.deleteConfirm')"
                  @ok="handleDeleteIpRule(record.id)"
                >
                  <a-button type="text" size="small" status="danger">
                    <template #icon><icon-delete /></template>
                  </a-button>
                </a-popconfirm>
              </a-space>
            </template>
          </a-table>
        </a-tab-pane>
      </a-tabs>
    </a-card>

    <!-- 限流策略表单弹窗 -->
    <a-modal
      v-model:visible="policyModalVisible"
      :title="policyForm.id ? $t('rateLimit.editPolicy') : $t('rateLimit.addPolicy')"
      @ok="handlePolicySubmit"
      @cancel="policyModalVisible = false"
      :ok-loading="policySubmitting"
    >
      <a-form :model="policyForm" layout="vertical">
        <a-form-item field="name" :label="$t('rateLimit.policyName')" required>
          <a-input v-model="policyForm.name" :disabled="!!policyForm.id" />
        </a-form-item>
        <a-form-item field="displayName" :label="$t('rateLimit.displayName')">
          <a-input v-model="policyForm.displayName" />
        </a-form-item>
        <a-form-item field="policyType" :label="$t('rateLimit.policyType')" required>
          <a-select v-model="policyForm.policyType">
            <a-option v-for="pt in policyTypes" :key="pt.value" :value="pt.value">
              {{ pt.label }}
            </a-option>
          </a-select>
        </a-form-item>
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item field="permitLimit" :label="$t('rateLimit.permitLimit')" required>
              <a-input-number v-model="policyForm.permitLimit" :min="1" style="width: 100%" />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="windowSeconds" :label="$t('rateLimit.windowSeconds')" required>
              <a-input-number v-model="policyForm.windowSeconds" :min="1" style="width: 100%" />
            </a-form-item>
          </a-col>
        </a-row>
        <a-form-item
          v-if="policyForm.policyType === 'sliding-window'"
          field="segmentsPerWindow"
          :label="$t('rateLimit.segments')"
        >
          <a-input-number v-model="policyForm.segmentsPerWindow" :min="1" />
        </a-form-item>
        <a-form-item field="queueLimit" :label="$t('rateLimit.queueLimit')">
          <a-input-number v-model="policyForm.queueLimit" :min="0" />
        </a-form-item>
        <a-form-item field="description" :label="$t('rateLimit.description')">
          <a-textarea v-model="policyForm.description" :auto-size="{ minRows: 2 }" />
        </a-form-item>
        <a-form-item field="isEnabled" :label="$t('rateLimit.enabled')">
          <a-switch v-model="policyForm.isEnabled" />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- IP 规则表单弹窗 -->
    <a-modal
      v-model:visible="ipModalVisible"
      :title="ipForm.id ? $t('rateLimit.editIpRule') : $t('rateLimit.addIpRule')"
      @ok="handleIpRuleSubmit"
      @cancel="ipModalVisible = false"
      :ok-loading="ipSubmitting"
    >
      <a-form :model="ipForm" layout="vertical">
        <a-form-item field="ipAddress" :label="$t('rateLimit.ipAddress')" required>
          <a-input v-model="ipForm.ipAddress" placeholder="192.168.1.1 或 192.168.1.0/24" />
        </a-form-item>
        <a-form-item field="ruleType" :label="$t('rateLimit.ruleType')" required>
          <a-radio-group v-model="ipForm.ruleType">
            <a-radio value="blacklist">{{ $t('rateLimit.blacklist') }}</a-radio>
            <a-radio value="whitelist">{{ $t('rateLimit.whitelist') }}</a-radio>
          </a-radio-group>
        </a-form-item>
        <a-form-item field="expiresAt" :label="$t('rateLimit.expiresAt')">
          <a-date-picker
            v-model="ipForm.expiresAt"
            show-time
            :placeholder="$t('rateLimit.expiresAtPlaceholder')"
            style="width: 100%"
          />
        </a-form-item>
        <a-form-item field="description" :label="$t('rateLimit.description')">
          <a-textarea v-model="ipForm.description" :auto-size="{ minRows: 2 }" />
        </a-form-item>
        <a-form-item field="isEnabled" :label="$t('rateLimit.enabled')">
          <a-switch v-model="ipForm.isEnabled" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { Message } from '@arco-design/web-vue';
import { useI18n } from 'vue-i18n';
import {
  getAllPolicies,
  createPolicy,
  updatePolicy,
  deletePolicy,
  getIpRules,
  createIpRule,
  updateIpRule,
  deleteIpRule,
  policyTypes,
  RateLimitPolicy,
  IpAccessRule,
} from '@/api/gateway/rate-limit';

const { t } = useI18n();

// ==================== 限流策略 ====================
const policyLoading = ref(false);
const policies = ref<RateLimitPolicy[]>([]);
const policyModalVisible = ref(false);
const policySubmitting = ref(false);
const policyForm = ref({
  id: '',
  name: '',
  displayName: '',
  policyType: 'fixed-window',
  permitLimit: 100,
  windowSeconds: 60,
  segmentsPerWindow: 6,
  queueLimit: 0,
  tokensPerPeriod: 10,
  replenishmentPeriodSeconds: 1,
  isEnabled: true,
  description: '',
});

const policyColumns = computed(() => [
  { title: t('rateLimit.policyName'), dataIndex: 'name' },
  { title: t('rateLimit.displayName'), dataIndex: 'displayName' },
  { title: t('rateLimit.policyType'), slotName: 'policyType' },
  { title: t('rateLimit.limit'), slotName: 'permitLimit' },
  { title: t('rateLimit.enabled'), slotName: 'isEnabled', width: 100 },
  { title: t('rateLimit.operations'), slotName: 'operations', width: 120 },
]);

const loadPolicies = async () => {
  policyLoading.value = true;
  try {
    const res = await getAllPolicies();
    if (res.data.success) {
      policies.value = res.data.data;
    }
  } finally {
    policyLoading.value = false;
  }
};

const getPolicyTypeColor = (type: string) => {
  const colors: Record<string, string> = {
    'fixed-window': 'blue',
    'sliding-window': 'green',
    'token-bucket': 'orange',
    concurrency: 'purple',
  };
  return colors[type] || 'gray';
};

const getPolicyTypeLabel = (type: string) => {
  const item = policyTypes.find((p) => p.value === type);
  return item?.label || type;
};

const handleAddPolicy = () => {
  policyForm.value = {
    id: '',
    name: '',
    displayName: '',
    policyType: 'fixed-window',
    permitLimit: 100,
    windowSeconds: 60,
    segmentsPerWindow: 6,
    queueLimit: 0,
    tokensPerPeriod: 10,
    replenishmentPeriodSeconds: 1,
    isEnabled: true,
    description: '',
  };
  policyModalVisible.value = true;
};

const handleEditPolicy = (record: RateLimitPolicy) => {
  policyForm.value = {
    id: record.id,
    name: record.name,
    displayName: record.displayName || '',
    policyType: record.policyType,
    permitLimit: record.permitLimit,
    windowSeconds: record.windowSeconds,
    segmentsPerWindow: record.segmentsPerWindow,
    queueLimit: record.queueLimit,
    tokensPerPeriod: record.tokensPerPeriod,
    replenishmentPeriodSeconds: record.replenishmentPeriodSeconds,
    isEnabled: record.isEnabled,
    description: record.description || '',
  };
  policyModalVisible.value = true;
};

const handlePolicySubmit = async () => {
  if (!policyForm.value.name) {
    Message.warning(t('rateLimit.nameRequired'));
    return;
  }

  policySubmitting.value = true;
  try {
    if (policyForm.value.id) {
      await updatePolicy(policyForm.value.id, policyForm.value);
      Message.success(t('rateLimit.updateSuccess'));
    } else {
      await createPolicy(policyForm.value);
      Message.success(t('rateLimit.createSuccess'));
    }
    policyModalVisible.value = false;
    loadPolicies();
  } catch (error) {
    Message.error(t('rateLimit.operationFailed'));
  } finally {
    policySubmitting.value = false;
  }
};

const handleTogglePolicy = async (record: RateLimitPolicy, enabled: boolean) => {
  try {
    await updatePolicy(record.id, { ...record, isEnabled: enabled });
    record.isEnabled = enabled;
    Message.success(t('rateLimit.updateSuccess'));
  } catch (error) {
    Message.error(t('rateLimit.operationFailed'));
  }
};

const handleDeletePolicy = async (id: string) => {
  try {
    await deletePolicy(id);
    Message.success(t('rateLimit.deleteSuccess'));
    loadPolicies();
  } catch (error) {
    Message.error(t('rateLimit.operationFailed'));
  }
};

// ==================== IP 访问规则 ====================
const ipLoading = ref(false);
const ipRules = ref<IpAccessRule[]>([]);
const ipModalVisible = ref(false);
const ipSubmitting = ref(false);
const ipFilter = reactive({
  ruleType: undefined as string | undefined,
  page: 1,
  pageSize: 20,
});
const ipTotal = ref(0);

const ipForm = ref({
  id: '',
  ipAddress: '',
  ruleType: 'blacklist' as 'whitelist' | 'blacklist',
  description: '',
  isEnabled: true,
  expiresAt: undefined as string | undefined,
});

const ipPagination = computed(() => ({
  current: ipFilter.page,
  pageSize: ipFilter.pageSize,
  total: ipTotal.value,
}));

const ipRuleColumns = computed(() => [
  { title: t('rateLimit.ipAddress'), dataIndex: 'ipAddress' },
  { title: t('rateLimit.ruleType'), slotName: 'ruleType', width: 100 },
  { title: t('rateLimit.description'), dataIndex: 'description', ellipsis: true },
  { title: t('rateLimit.expiresAt'), slotName: 'expiresAt', width: 160 },
  { title: t('rateLimit.enabled'), slotName: 'isEnabled', width: 80 },
  { title: t('rateLimit.operations'), slotName: 'operations', width: 120 },
]);

const loadIpRules = async () => {
  ipLoading.value = true;
  try {
    const res = await getIpRules({
      ruleType: ipFilter.ruleType,
      page: ipFilter.page,
      pageSize: ipFilter.pageSize,
    });
    if (res.data.success) {
      ipRules.value = res.data.data.items;
      ipTotal.value = res.data.data.total;
    }
  } finally {
    ipLoading.value = false;
  }
};

const handleIpPageChange = (page: number) => {
  ipFilter.page = page;
  loadIpRules();
};

const handleAddIpRule = () => {
  ipForm.value = {
    id: '',
    ipAddress: '',
    ruleType: 'blacklist',
    description: '',
    isEnabled: true,
    expiresAt: undefined,
  };
  ipModalVisible.value = true;
};

const handleEditIpRule = (record: IpAccessRule) => {
  ipForm.value = {
    id: record.id,
    ipAddress: record.ipAddress,
    ruleType: record.ruleType,
    description: record.description || '',
    isEnabled: record.isEnabled,
    expiresAt: record.expiresAt,
  };
  ipModalVisible.value = true;
};

const handleIpRuleSubmit = async () => {
  if (!ipForm.value.ipAddress) {
    Message.warning(t('rateLimit.ipRequired'));
    return;
  }

  ipSubmitting.value = true;
  try {
    if (ipForm.value.id) {
      await updateIpRule(ipForm.value.id, ipForm.value);
      Message.success(t('rateLimit.updateSuccess'));
    } else {
      await createIpRule(ipForm.value);
      Message.success(t('rateLimit.createSuccess'));
    }
    ipModalVisible.value = false;
    loadIpRules();
  } catch (error) {
    Message.error(t('rateLimit.operationFailed'));
  } finally {
    ipSubmitting.value = false;
  }
};

const handleToggleIpRule = async (record: IpAccessRule, enabled: boolean) => {
  try {
    await updateIpRule(record.id, { ...record, isEnabled: enabled });
    record.isEnabled = enabled;
    Message.success(t('rateLimit.updateSuccess'));
  } catch (error) {
    Message.error(t('rateLimit.operationFailed'));
  }
};

const handleDeleteIpRule = async (id: string) => {
  try {
    await deleteIpRule(id);
    Message.success(t('rateLimit.deleteSuccess'));
    loadIpRules();
  } catch (error) {
    Message.error(t('rateLimit.operationFailed'));
  }
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString();
};

onMounted(() => {
  loadPolicies();
  loadIpRules();
});
</script>

<style scoped lang="less">
.container {
  padding: 16px;
}

.toolbar {
  margin-bottom: 16px;
}

.text-secondary {
  color: var(--color-text-3);
}
</style>
