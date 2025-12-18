<template>
  <div class="gateway-route-container">
    <a-card class="general-card" :title="$t('menu.gateway.route')">
      <!-- 搜索表单 -->
      <a-row :gutter="16" style="margin-bottom: 16px">
        <a-col :span="5">
          <a-input
            v-model="queryParams.keyword"
            :placeholder="$t('gateway.route.searchPlaceholder')"
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
            v-model="queryParams.clusterId"
            :placeholder="$t('gateway.route.clusterId')"
            allow-clear
            @change="handleSearch"
          >
            <a-option
              v-for="cluster in clusterOptions"
              :key="cluster.clusterId"
              :value="cluster.clusterId"
            >
              {{ cluster.name }} ({{ cluster.clusterId }})
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
        <a-col :span="11">
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
        <template #routeId="{ record }">
          <a-tag color="blue">{{ record.routeId }}</a-tag>
        </template>
        <template #clusterId="{ record }">
          <a-tag color="cyan">{{ record.clusterId }}</a-tag>
        </template>
        <template #matchPath="{ record }">
          <a-tooltip :content="record.matchPath">
            <span class="path-text">{{ record.matchPath }}</span>
          </a-tooltip>
        </template>
        <template #matchMethods="{ record }">
          <a-space v-if="record.matchMethods" wrap>
            <a-tag
              v-for="method in record.matchMethods.split(',')"
              :key="method"
              :color="getMethodColor(method)"
              size="small"
            >
              {{ method }}
            </a-tag>
          </a-space>
          <span v-else style="color: #86909c">全部</span>
        </template>
        <template #policies="{ record }">
          <a-space wrap>
            <a-tag
              v-if="record.authorizationPolicy"
              color="orange"
              size="small"
            >
              {{ record.authorizationPolicy }}
            </a-tag>
            <a-tag v-if="record.rateLimiterPolicy" color="purple" size="small">
              {{ record.rateLimiterPolicy }}
            </a-tag>
          </a-space>
        </template>
        <template #order="{ record }">
          <a-tag>{{ record.sortOrder }}</a-tag>
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
            <a-form-item field="routeId" :label="$t('gateway.route.routeId')">
              <a-input
                v-model="formData.routeId"
                :disabled="isEdit"
                :placeholder="$t('gateway.route.routeIdPlaceholder')"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="name" :label="$t('gateway.route.name')">
              <a-input
                v-model="formData.name"
                :placeholder="$t('gateway.route.namePlaceholder')"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-form-item
          field="description"
          :label="$t('gateway.route.description')"
        >
          <a-textarea
            v-model="formData.description"
            :placeholder="$t('gateway.route.descriptionPlaceholder')"
            :max-length="1000"
            show-word-limit
          />
        </a-form-item>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item
              field="clusterId"
              :label="$t('gateway.route.clusterId')"
            >
              <a-select
                v-model="formData.clusterId"
                :placeholder="$t('gateway.route.clusterIdPlaceholder')"
              >
                <a-option
                  v-for="cluster in clusterOptions"
                  :key="cluster.clusterId"
                  :value="cluster.clusterId"
                >
                  {{ cluster.name }} ({{ cluster.clusterId }})
                </a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item
              field="matchPath"
              :label="$t('gateway.route.matchPath')"
            >
              <a-input
                v-model="formData.matchPath"
                :placeholder="$t('gateway.route.matchPathPlaceholder')"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item
              field="matchMethods"
              :label="$t('gateway.route.matchMethods')"
            >
              <a-select
                v-model="formData.matchMethodsArray"
                multiple
                :placeholder="$t('gateway.route.matchMethodsPlaceholder')"
              >
                <a-option value="GET">GET</a-option>
                <a-option value="POST">POST</a-option>
                <a-option value="PUT">PUT</a-option>
                <a-option value="DELETE">DELETE</a-option>
                <a-option value="PATCH">PATCH</a-option>
                <a-option value="OPTIONS">OPTIONS</a-option>
                <a-option value="HEAD">HEAD</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="sortOrder" :label="$t('gateway.route.order')">
              <a-input-number
                v-model="formData.sortOrder"
                :min="0"
                :placeholder="$t('gateway.route.orderPlaceholder')"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-divider>{{ $t('gateway.route.policies') }}</a-divider>

        <a-row :gutter="16">
          <a-col :span="8">
            <a-form-item
              field="authorizationPolicy"
              :label="$t('gateway.route.authorizationPolicy')"
            >
              <a-select v-model="formData.authorizationPolicy" allow-clear>
                <a-option
                  v-for="policy in authorizationPolicies"
                  :key="policy.value"
                  :value="policy.value"
                >
                  {{ policy.label }}
                </a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item
              field="rateLimiterPolicy"
              :label="$t('gateway.route.rateLimiterPolicy')"
            >
              <a-select v-model="formData.rateLimiterPolicy" allow-clear>
                <a-option
                  v-for="policy in rateLimiterPolicies"
                  :key="policy.value"
                  :value="policy.value"
                >
                  {{ policy.label }}
                </a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item
              field="timeoutSeconds"
              :label="$t('gateway.route.timeoutSeconds')"
            >
              <a-input-number
                v-model="formData.timeoutSeconds"
                :min="1"
                placeholder="60"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-divider>{{ $t('gateway.route.transforms') }}</a-divider>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item
              field="transformPathPrefix"
              :label="$t('gateway.route.transformPathPrefix')"
            >
              <a-input
                v-model="formData.transformPathPrefix"
                placeholder="/api"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item
              field="transformPathRemovePrefix"
              :label="$t('gateway.route.transformPathRemovePrefix')"
            >
              <a-input
                v-model="formData.transformPathRemovePrefix"
                placeholder="/gateway"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item
              field="isEnabled"
              :label="$t('gateway.route.isEnabled')"
            >
              <a-switch v-model="formData.isEnabled" />
            </a-form-item>
          </a-col>
        </a-row>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted, watch } from 'vue';
  import { Message } from '@arco-design/web-vue';
  import { useI18n } from 'vue-i18n';
  import type { TableColumnData } from '@arco-design/web-vue';
  import {
    getRoutePagedList,
    createRoute,
    updateRoute,
    deleteRoute,
    toggleRouteEnabled,
    getClusterOptions,
    authorizationPolicies,
    rateLimiterPolicies,
    type GatewayRoute,
    type GatewayRouteQueryParams,
    type ClusterOption,
  } from '@/api/gateway/gateway';

  const { t } = useI18n();

  // 查询参数
  const queryParams = reactive<GatewayRouteQueryParams>({
    keyword: '',
    clusterId: undefined,
    isEnabled: undefined,
    page: 1,
    pageSize: 10,
  });

  // 集群选项
  const clusterOptions = ref<ClusterOption[]>([]);

  // 表格数据
  const tableData = ref<GatewayRoute[]>([]);
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
    { title: t('gateway.route.routeId'), slotName: 'routeId', width: 150 },
    { title: t('gateway.route.name'), dataIndex: 'name', width: 120 },
    { title: t('gateway.route.clusterId'), slotName: 'clusterId', width: 120 },
    { title: t('gateway.route.matchPath'), slotName: 'matchPath', width: 180 },
    {
      title: t('gateway.route.matchMethods'),
      slotName: 'matchMethods',
      width: 150,
    },
    { title: t('gateway.route.policies'), slotName: 'policies', width: 180 },
    { title: t('gateway.route.order'), slotName: 'order', width: 60 },
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
    routeId: '',
    name: '',
    description: '',
    clusterId: '',
    matchPath: '',
    matchMethods: '',
    matchMethodsArray: [] as string[],
    matchHosts: '',
    matchHeaders: '',
    matchQueryParameters: '',
    transformPathPrefix: '',
    transformPathRemovePrefix: '',
    transformRequestHeaders: '',
    transformResponseHeaders: '',
    authorizationPolicy: '',
    rateLimiterPolicy: '',
    corsPolicy: '',
    timeoutSeconds: undefined as number | undefined,
    sortOrder: 0,
    isEnabled: true,
    metadata: '',
  });

  // 监听 matchMethodsArray 变化，同步到 matchMethods
  watch(
    () => formData.matchMethodsArray,
    (val) => {
      formData.matchMethods = val.join(',');
    }
  );

  // 表单验证规则
  const formRules = {
    routeId: [{ required: true, message: t('gateway.route.routeIdRequired') }],
    name: [{ required: true, message: t('gateway.route.nameRequired') }],
    clusterId: [
      { required: true, message: t('gateway.route.clusterIdRequired') },
    ],
    matchPath: [
      { required: true, message: t('gateway.route.matchPathRequired') },
    ],
  };

  // 获取HTTP方法颜色
  const getMethodColor = (method: string) => {
    const colors: Record<string, string> = {
      GET: 'green',
      POST: 'blue',
      PUT: 'orange',
      DELETE: 'red',
      PATCH: 'purple',
      OPTIONS: 'gray',
      HEAD: 'cyan',
    };
    return colors[method.toUpperCase()] || 'gray';
  };

  // 加载集群选项
  const loadClusterOptions = async () => {
    try {
      clusterOptions.value = await getClusterOptions();
    } catch (error) {
      console.error('Failed to load cluster options:', error);
    }
  };

  // 加载数据
  const loadData = async () => {
    loading.value = true;
    try {
      const result = await getRoutePagedList(queryParams);
      tableData.value = result.items as GatewayRoute[];
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
    queryParams.clusterId = undefined;
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

  // 重置表单
  const resetForm = () => {
    formData.id = '';
    formData.routeId = '';
    formData.name = '';
    formData.description = '';
    formData.clusterId = '';
    formData.matchPath = '';
    formData.matchMethods = '';
    formData.matchMethodsArray = [];
    formData.transformPathPrefix = '';
    formData.transformPathRemovePrefix = '';
    formData.authorizationPolicy = '';
    formData.rateLimiterPolicy = '';
    formData.timeoutSeconds = undefined;
    formData.sortOrder = 0;
    formData.isEnabled = true;
    formRef.value?.resetFields();
  };

  // 创建
  const handleCreate = () => {
    isEdit.value = false;
    resetForm();
    modalVisible.value = true;
  };

  // 编辑
  const handleEdit = (record: GatewayRoute) => {
    isEdit.value = true;
    Object.assign(formData, record);
    formData.matchMethodsArray = record.matchMethods
      ? record.matchMethods.split(',')
      : [];
    modalVisible.value = true;
  };

  // 删除
  const handleDelete = async (record: GatewayRoute) => {
    try {
      await deleteRoute(record.id);
      Message.success(t('common.deleteSuccess'));
      loadData();
    } catch (error: any) {
      Message.error(error.response?.data?.message || t('common.deleteFailed'));
    }
  };

  // 切换启用状态
  const handleToggleEnabled = async (
    record: GatewayRoute,
    isEnabled: boolean
  ) => {
    try {
      await toggleRouteEnabled(record.id, isEnabled);
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
      const submitData = { ...formData };
      delete (submitData as any).matchMethodsArray;

      if (isEdit.value) {
        await updateRoute(formData.id, submitData as any);
        Message.success(t('common.updateSuccess'));
      } else {
        await createRoute(submitData as any);
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

  onMounted(() => {
    loadClusterOptions();
    loadData();
  });
</script>

<style scoped>
  .gateway-route-container {
    padding: 16px;
  }

  .path-text {
    max-width: 160px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    display: inline-block;
  }
</style>
