<template>
  <div class="gateway-cluster-container">
    <a-card class="general-card" :title="$t('menu.gateway.cluster')">
      <!-- 搜索表单 -->
      <a-row :gutter="16" style="margin-bottom: 16px">
        <a-col :span="6">
          <a-input
            v-model="queryParams.keyword"
            :placeholder="$t('gateway.cluster.searchPlaceholder')"
            allow-clear
            @press-enter="handleSearch"
          >
            <template #prefix>
              <icon-search />
            </template>
          </a-input>
        </a-col>
        <a-col :span="4">
          <a-select
            v-model="queryParams.loadBalancingPolicy"
            :placeholder="$t('gateway.cluster.loadBalancingPolicy')"
            allow-clear
            @change="handleSearch"
          >
            <a-option
              v-for="policy in loadBalancingPolicies"
              :key="policy.value"
              :value="policy.value"
            >
              {{ policy.label }}
            </a-option>
          </a-select>
        </a-col>
        <a-col :span="4">
          <a-select
            v-model="queryParams.isEnabled"
            :placeholder="$t('gateway.status')"
            allow-clear
            @change="handleSearch"
          >
            <a-option :value="true">{{ $t('gateway.enabled') }}</a-option>
            <a-option :value="false">{{ $t('gateway.disabled') }}</a-option>
          </a-select>
        </a-col>
        <a-col :span="10">
          <a-space>
            <a-button type="primary" @click="handleSearch">
              <template #icon><icon-search /></template>
              {{ $t('common.search') }}
            </a-button>
            <a-button @click="handleReset">
              <template #icon><icon-refresh /></template>
              {{ $t('common.reset') }}
            </a-button>
            <a-button type="primary" status="success" @click="handleCreate">
              <template #icon><icon-plus /></template>
              {{ $t('common.create') }}
            </a-button>
          </a-space>
        </a-col>
      </a-row>

      <!-- 数据表格 -->
      <a-table
        :columns="columns"
        :data="tableData"
        :loading="loading"
        :pagination="pagination"
        row-key="id"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      >
        <template #clusterId="{ record }">
          <a-tag color="blue">{{ record.clusterId }}</a-tag>
        </template>
        <template #loadBalancingPolicy="{ record }">
          <a-tag>{{ getLoadBalancingLabel(record.loadBalancingPolicy) }}</a-tag>
        </template>
        <template #destinations="{ record }">
          <a-popover trigger="click" position="bottom">
            <a-button type="text" size="small">
              {{ getDestinationCount(record.destinations) }} 个目标
            </a-button>
            <template #content>
              <div style="max-width: 400px">
                <pre style="margin: 0; font-size: 12px">{{
                  formatDestinations(record.destinations)
                }}</pre>
              </div>
            </template>
          </a-popover>
        </template>
        <template #healthCheck="{ record }">
          <a-tag :color="record.healthCheckEnabled ? 'green' : 'gray'">
            {{ record.healthCheckEnabled ? '启用' : '禁用' }}
          </a-tag>
        </template>
        <template #isEnabled="{ record }">
          <a-switch
            :model-value="record.isEnabled"
            @change="(val) => handleToggleEnabled(record, val as boolean)"
          />
        </template>
        <template #operations="{ record }">
          <a-space>
            <a-button type="text" size="small" @click="handleEdit(record)">
              <template #icon><icon-edit /></template>
              {{ $t('common.edit') }}
            </a-button>
            <a-popconfirm
              :content="$t('common.deleteConfirm')"
              @ok="handleDelete(record)"
            >
              <a-button type="text" size="small" status="danger">
                <template #icon><icon-delete /></template>
                {{ $t('common.delete') }}
              </a-button>
            </a-popconfirm>
          </a-space>
        </template>
      </a-table>
    </a-card>

    <!-- 创建/编辑弹窗 -->
    <a-modal
      v-model:visible="modalVisible"
      :title="isEdit ? $t('common.edit') : $t('common.create')"
      :width="800"
      :ok-loading="submitLoading"
      @ok="handleSubmit"
      @cancel="handleCancel"
    >
      <a-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        layout="vertical"
      >
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item
              field="clusterId"
              :label="$t('gateway.cluster.clusterId')"
            >
              <a-input
                v-model="formData.clusterId"
                :disabled="isEdit"
                :placeholder="$t('gateway.cluster.clusterIdPlaceholder')"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="name" :label="$t('gateway.cluster.name')">
              <a-input
                v-model="formData.name"
                :placeholder="$t('gateway.cluster.namePlaceholder')"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-form-item
          field="description"
          :label="$t('gateway.cluster.description')"
        >
          <a-textarea
            v-model="formData.description"
            :placeholder="$t('gateway.cluster.descriptionPlaceholder')"
            :max-length="1000"
            show-word-limit
          />
        </a-form-item>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item
              field="loadBalancingPolicy"
              :label="$t('gateway.cluster.loadBalancingPolicy')"
            >
              <a-select v-model="formData.loadBalancingPolicy">
                <a-option
                  v-for="policy in loadBalancingPolicies"
                  :key="policy.value"
                  :value="policy.value"
                >
                  {{ policy.label }}
                </a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item
              field="isEnabled"
              :label="$t('gateway.cluster.isEnabled')"
            >
              <a-switch v-model="formData.isEnabled" />
            </a-form-item>
          </a-col>
        </a-row>

        <a-divider>{{ $t('gateway.cluster.destinations') }}</a-divider>

        <a-form-item field="destinations">
          <a-textarea
            v-model="formData.destinations"
            :placeholder="$t('gateway.cluster.destinationsPlaceholder')"
            :auto-size="{ minRows: 4, maxRows: 8 }"
          />
          <template #extra>
            <span style="color: #86909c; font-size: 12px">
              JSON格式，例如: [{"destinationId": "dest1", "address":
              "http://localhost:5000"}]
            </span>
          </template>
        </a-form-item>

        <a-divider>{{ $t('gateway.cluster.healthCheck') }}</a-divider>

        <a-row :gutter="16">
          <a-col :span="6">
            <a-form-item
              field="healthCheckEnabled"
              :label="$t('gateway.cluster.healthCheckEnabled')"
            >
              <a-switch v-model="formData.healthCheckEnabled" />
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item
              field="healthCheckPath"
              :label="$t('gateway.cluster.healthCheckPath')"
            >
              <a-input
                v-model="formData.healthCheckPath"
                :disabled="!formData.healthCheckEnabled"
                placeholder="/health"
              />
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item
              field="healthCheckInterval"
              :label="$t('gateway.cluster.healthCheckInterval')"
            >
              <a-input-number
                v-model="formData.healthCheckInterval"
                :disabled="!formData.healthCheckEnabled"
                :min="1"
                placeholder="30"
              />
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item
              field="healthCheckTimeout"
              :label="$t('gateway.cluster.healthCheckTimeout')"
            >
              <a-input-number
                v-model="formData.healthCheckTimeout"
                :disabled="!formData.healthCheckEnabled"
                :min="1"
                placeholder="10"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-divider>{{ $t('gateway.cluster.httpClient') }}</a-divider>

        <a-row :gutter="16">
          <a-col :span="8">
            <a-form-item
              field="maxConnectionsPerServer"
              :label="$t('gateway.cluster.maxConnections')"
            >
              <a-input-number
                v-model="formData.maxConnectionsPerServer"
                :min="1"
                placeholder="100"
              />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item
              field="requestTimeoutSeconds"
              :label="$t('gateway.cluster.requestTimeout')"
            >
              <a-input-number
                v-model="formData.requestTimeoutSeconds"
                :min="1"
                placeholder="60"
              />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item
              field="dangerousAcceptAnyServerCertificate"
              :label="$t('gateway.cluster.skipCertValidation')"
            >
              <a-switch
                v-model="formData.dangerousAcceptAnyServerCertificate"
              />
            </a-form-item>
          </a-col>
        </a-row>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted } from 'vue';
  import { Message } from '@arco-design/web-vue';
  import { useI18n } from 'vue-i18n';
  import type { TableColumnData } from '@arco-design/web-vue';
  import {
    getClusterPagedList,
    createCluster,
    updateCluster,
    deleteCluster,
    toggleClusterEnabled,
    loadBalancingPolicies,
    type GatewayCluster,
    type GatewayClusterQueryParams,
  } from '@/api/gateway/gateway';

  const { t } = useI18n();

  // 查询参数
  const queryParams = reactive<GatewayClusterQueryParams>({
    keyword: '',
    loadBalancingPolicy: undefined,
    isEnabled: undefined,
    page: 1,
    pageSize: 10,
  });

  // 表格数据
  const tableData = ref<GatewayCluster[]>([]);
  const loading = ref(false);
  const total = ref(0);

  // 分页配置
  const pagination = computed(() => ({
    total: total.value,
    current: queryParams.page,
    pageSize: queryParams.pageSize,
    showTotal: true,
    showJumper: true,
    showPageSize: true,
  }));

  // 表格列配置
  const columns: TableColumnData[] = [
    {
      title: t('gateway.cluster.clusterId'),
      slotName: 'clusterId',
      width: 150,
    },
    { title: t('gateway.cluster.name'), dataIndex: 'name', width: 150 },
    {
      title: t('gateway.cluster.loadBalancingPolicy'),
      slotName: 'loadBalancingPolicy',
      width: 150,
    },
    {
      title: t('gateway.cluster.destinations'),
      slotName: 'destinations',
      width: 100,
    },
    {
      title: t('gateway.cluster.healthCheck'),
      slotName: 'healthCheck',
      width: 100,
    },
    { title: t('gateway.status'), slotName: 'isEnabled', width: 80 },
    { title: t('common.operations'), slotName: 'operations', width: 160 },
  ];

  // 弹窗相关
  const modalVisible = ref(false);
  const isEdit = ref(false);
  const submitLoading = ref(false);
  const formRef = ref();

  // 表单数据
  const formData = reactive({
    id: '',
    clusterId: '',
    name: '',
    description: '',
    loadBalancingPolicy: 'RoundRobin',
    destinations: '[]',
    healthCheckEnabled: false,
    healthCheckInterval: undefined as number | undefined,
    healthCheckTimeout: undefined as number | undefined,
    healthCheckPath: '',
    passiveHealthPolicy: '',
    sessionAffinityEnabled: false,
    sessionAffinityPolicy: '',
    sessionAffinityKeyName: '',
    maxConnectionsPerServer: undefined as number | undefined,
    requestTimeoutSeconds: undefined as number | undefined,
    allowedHttpVersions: '',
    dangerousAcceptAnyServerCertificate: false,
    metadata: '',
    isEnabled: true,
  });

  // 表单验证规则
  const formRules = {
    clusterId: [
      { required: true, message: t('gateway.cluster.clusterIdRequired') },
    ],
    name: [{ required: true, message: t('gateway.cluster.nameRequired') }],
  };

  // 获取负载均衡策略标签
  const getLoadBalancingLabel = (value: string) => {
    const policy = loadBalancingPolicies.find((p) => p.value === value);
    return policy?.label || value;
  };

  // 获取目标服务器数量
  const getDestinationCount = (destinations: string) => {
    try {
      const arr = JSON.parse(destinations);
      return Array.isArray(arr) ? arr.length : 0;
    } catch {
      return 0;
    }
  };

  // 格式化目标服务器列表
  const formatDestinations = (destinations: string) => {
    try {
      return JSON.stringify(JSON.parse(destinations), null, 2);
    } catch {
      return destinations;
    }
  };

  // 加载数据
  const loadData = async () => {
    loading.value = true;
    try {
      const result = await getClusterPagedList(queryParams);
      tableData.value = result.items as GatewayCluster[];
      total.value = result.totalCount;
    } catch (error) {
      Message.error(t('common.loadFailed'));
    } finally {
      loading.value = false;
    }
  };

  // 搜索
  const handleSearch = () => {
    queryParams.page = 1;
    loadData();
  };

  // 重置
  const handleReset = () => {
    queryParams.keyword = '';
    queryParams.loadBalancingPolicy = undefined;
    queryParams.isEnabled = undefined;
    queryParams.page = 1;
    loadData();
  };

  // 分页
  const handlePageChange = (page: number) => {
    queryParams.page = page;
    loadData();
  };

  const handlePageSizeChange = (pageSize: number) => {
    queryParams.pageSize = pageSize;
    queryParams.page = 1;
    loadData();
  };

  // 创建
  const handleCreate = () => {
    isEdit.value = false;
    resetForm();
    modalVisible.value = true;
  };

  // 编辑
  const handleEdit = (record: GatewayCluster) => {
    isEdit.value = true;
    Object.assign(formData, record);
    modalVisible.value = true;
  };

  // 删除
  const handleDelete = async (record: GatewayCluster) => {
    try {
      await deleteCluster(record.id);
      Message.success(t('common.deleteSuccess'));
      loadData();
    } catch (error: any) {
      Message.error(error.response?.data?.message || t('common.deleteFailed'));
    }
  };

  // 切换启用状态
  const handleToggleEnabled = async (
    record: GatewayCluster,
    isEnabled: boolean
  ) => {
    try {
      await toggleClusterEnabled(record.id, isEnabled);
      Message.success(
        isEnabled ? t('gateway.enableSuccess') : t('gateway.disableSuccess')
      );
      loadData();
    } catch (error) {
      Message.error(t('common.operationFailed'));
    }
  };

  // 提交表单
  const handleSubmit = async () => {
    const valid = await formRef.value?.validate();
    if (valid) return;

    submitLoading.value = true;
    try {
      if (isEdit.value) {
        await updateCluster(formData.id, formData as any);
        Message.success(t('common.updateSuccess'));
      } else {
        await createCluster(formData as any);
        Message.success(t('common.createSuccess'));
      }
      modalVisible.value = false;
      loadData();
    } catch (error: any) {
      Message.error(
        error.response?.data?.message || t('common.operationFailed')
      );
    } finally {
      submitLoading.value = false;
    }
  };

  // 取消
  const handleCancel = () => {
    modalVisible.value = false;
    resetForm();
  };

  // 重置表单
  const resetForm = () => {
    formData.id = '';
    formData.clusterId = '';
    formData.name = '';
    formData.description = '';
    formData.loadBalancingPolicy = 'RoundRobin';
    formData.destinations = '[]';
    formData.healthCheckEnabled = false;
    formData.healthCheckInterval = undefined;
    formData.healthCheckTimeout = undefined;
    formData.healthCheckPath = '';
    formData.maxConnectionsPerServer = undefined;
    formData.requestTimeoutSeconds = undefined;
    formData.dangerousAcceptAnyServerCertificate = false;
    formData.isEnabled = true;
    formRef.value?.resetFields();
  };

  onMounted(() => {
    loadData();
  });
</script>

<style scoped>
  .gateway-cluster-container {
    padding: 16px;
  }
</style>
