<template>
  <div class="container">
    <Breadcrumb :items="['menu.ids', 'menu.ids.apiResource']" />
    <a-card class="general-card" :title="$t('menu.ids.apiResource')">
      <!-- 搜索表单 -->
      <a-row style="margin-bottom: 16px">
        <a-col :span="24">
          <a-form :model="searchForm" layout="inline">
            <a-form-item field="name" :label="$t('ids.apiResource.name')">
              <a-input
                v-model="searchForm.name"
                :placeholder="$t('ids.apiResource.name.placeholder')"
                allow-clear
                style="width: 200px"
              />
            </a-form-item>
            <a-form-item
              field="displayName"
              :label="$t('ids.apiResource.displayName')"
            >
              <a-input
                v-model="searchForm.displayName"
                :placeholder="$t('ids.apiResource.displayName.placeholder')"
                allow-clear
                style="width: 200px"
              />
            </a-form-item>
            <a-form-item
              field="enabled"
              :label="$t('ids.apiResource.enabled')"
            >
              <a-select
                v-model="searchForm.enabled"
                :placeholder="$t('ids.apiResource.enabled.placeholder')"
                allow-clear
                style="width: 120px"
              >
                <a-option :value="true">{{ $t('common.yes') }}</a-option>
                <a-option :value="false">{{ $t('common.no') }}</a-option>
              </a-select>
            </a-form-item>
            <a-form-item field="scope" :label="$t('ids.apiResource.scope')">
              <a-input
                v-model="searchForm.scope"
                :placeholder="$t('ids.apiResource.scope.placeholder')"
                allow-clear
                style="width: 200px"
              />
            </a-form-item>
            <a-form-item>
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
            </a-form-item>
          </a-form>
        </a-col>
      </a-row>

      <!-- 操作按钮 -->
      <a-row style="margin-bottom: 16px">
        <a-col :span="24">
          <a-space>
            <a-button type="primary" @click="handleCreate">
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
        :pagination="pagination"
        :loading="loading"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      >
        <template #name="{ record }">
          <a-tag color="blue">{{ record.name }}</a-tag>
        </template>
        <template #enabled="{ record }">
          <a-tag :color="record.enabled ? 'green' : 'gray'">
            {{ record.enabled ? $t('common.yes') : $t('common.no') }}
          </a-tag>
        </template>
        <template #scopes="{ record }">
          <a-space wrap>
            <a-tag
              v-for="scope in record.scopes.slice(0, 3)"
              :key="scope"
              color="arcoblue"
            >
              {{ scope }}
            </a-tag>
            <a-tag v-if="record.scopes.length > 3" color="gray">
              +{{ record.scopes.length - 3 }}
            </a-tag>
          </a-space>
        </template>
        <template #userClaims="{ record }">
          <a-space wrap>
            <a-tag
              v-for="claim in record.userClaims.slice(0, 2)"
              :key="claim"
              color="purple"
            >
              {{ claim }}
            </a-tag>
            <a-tag v-if="record.userClaims.length > 2" color="gray">
              +{{ record.userClaims.length - 2 }}
            </a-tag>
          </a-space>
        </template>
        <template #createdAt="{ record }">
          {{ formatDateTime(record.createdAt) }}
        </template>
        <template #operations="{ record }">
          <a-space>
            <a-button type="text" size="small" @click="handleView(record)">
              {{ $t('common.view') }}
            </a-button>
            <a-button type="text" size="small" @click="handleEdit(record)">
              {{ $t('common.edit') }}
            </a-button>
            <a-popconfirm
              :content="$t('common.deleteConfirm')"
              @ok="handleDelete(record)"
            >
              <a-button type="text" size="small" status="danger">
                {{ $t('common.delete') }}
              </a-button>
            </a-popconfirm>
          </a-space>
        </template>
      </a-table>
    </a-card>

    <!-- 创建/编辑对话框 -->
    <a-modal
      v-model:visible="modalVisible"
      :title="modalTitle"
      :width="1000"
      @cancel="handleModalCancel"
      @before-ok="handleModalOk"
    >
      <a-form
        :model="formData"
        :rules="formRules"
        ref="formRef"
        layout="vertical"
      >
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item
              field="name"
              :label="$t('ids.apiResource.name')"
              required
            >
              <a-input
                v-model="formData.name"
                :placeholder="$t('ids.apiResource.name.placeholder')"
                :disabled="isView"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item
              field="displayName"
              :label="$t('ids.apiResource.displayName')"
              required
            >
              <a-input
                v-model="formData.displayName"
                :placeholder="$t('ids.apiResource.displayName.placeholder')"
                :disabled="isView"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-form-item
          field="description"
          :label="$t('ids.apiResource.description')"
        >
          <a-textarea
            v-model="formData.description"
            :placeholder="$t('ids.apiResource.description.placeholder')"
            :auto-size="{ minRows: 2, maxRows: 4 }"
            :disabled="isView"
          />
        </a-form-item>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item
              field="enabled"
              :label="$t('ids.apiResource.enabled')"
            >
              <a-switch v-model="formData.enabled" :disabled="isView" />
              <div class="form-help-text">
                {{ $t('ids.apiResource.enabled.help') }}
              </div>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item
              field="showInDiscoveryDocument"
              :label="$t('ids.apiResource.showInDiscoveryDocument')"
            >
              <a-switch
                v-model="formData.showInDiscoveryDocument"
                :disabled="isView"
              />
              <div class="form-help-text">
                {{ $t('ids.apiResource.showInDiscoveryDocument.help') }}
              </div>
            </a-form-item>
          </a-col>
        </a-row>

        <a-divider orientation="left">{{
          $t('ids.apiResource.scopes')
        }}</a-divider>
        <a-form-item>
          <a-space direction="vertical" fill>
            <a-space v-for="(scope, index) in formData.scopes" :key="index">
              <a-input
                v-model="formData.scopes[index]"
                :placeholder="$t('ids.apiResource.scopes.placeholder')"
                style="width: 400px"
                :disabled="isView"
              />
              <a-button
                type="text"
                status="danger"
                :disabled="isView"
                @click="handleRemoveScope(index)"
              >
                <template #icon><icon-delete /></template>
              </a-button>
            </a-space>
            <a-button v-if="!isView" type="dashed" long @click="handleAddScope">
              <template #icon><icon-plus /></template>
              {{ $t('ids.apiResource.scopes.add') }}
            </a-button>
          </a-space>
        </a-form-item>

        <a-divider orientation="left">{{
          $t('ids.apiResource.userClaims')
        }}</a-divider>
        <a-form-item>
          <a-space direction="vertical" fill>
            <a-space v-for="(claim, index) in formData.userClaims" :key="index">
              <a-input
                v-model="formData.userClaims[index]"
                :placeholder="$t('ids.apiResource.userClaims.placeholder')"
                style="width: 400px"
                :disabled="isView"
              />
              <a-button
                type="text"
                status="danger"
                :disabled="isView"
                @click="handleRemoveClaim(index)"
              >
                <template #icon><icon-delete /></template>
              </a-button>
            </a-space>
            <a-button v-if="!isView" type="dashed" long @click="handleAddClaim">
              <template #icon><icon-plus /></template>
              {{ $t('ids.apiResource.userClaims.add') }}
            </a-button>
          </a-space>
        </a-form-item>

        <a-divider orientation="left">{{
          $t('ids.apiResource.allowedAccessTokenSigningAlgorithms')
        }}</a-divider>
        <a-form-item>
          <a-space direction="vertical" fill>
            <a-space
              v-for="(alg, index) in formData.allowedAccessTokenSigningAlgorithms"
              :key="index"
            >
              <a-input
                v-model="formData.allowedAccessTokenSigningAlgorithms[index]"
                :placeholder="
                  $t(
                    'ids.apiResource.allowedAccessTokenSigningAlgorithms.placeholder'
                  )
                "
                style="width: 400px"
                :disabled="isView"
              />
              <a-button
                type="text"
                status="danger"
                :disabled="isView"
                @click="handleRemoveAlgorithm(index)"
              >
                <template #icon><icon-delete /></template>
              </a-button>
            </a-space>
            <a-button
              v-if="!isView"
              type="dashed"
              long
              @click="handleAddAlgorithm"
            >
              <template #icon><icon-plus /></template>
              {{
                $t('ids.apiResource.allowedAccessTokenSigningAlgorithms.add')
              }}
            </a-button>
          </a-space>
        </a-form-item>

        <a-divider orientation="left">{{
          $t('ids.apiResource.properties')
        }}</a-divider>
        <a-form-item>
          <a-space direction="vertical" fill>
            <a-space
              v-for="(value, key, index) in formData.properties"
              :key="index"
            >
              <a-input
                :model-value="key"
                :placeholder="$t('ids.apiResource.properties.key')"
                style="width: 200px"
                :disabled="isView"
                @change="(val: string) => handlePropertyKeyChange(key, val)"
              />
              <a-input
                v-model="formData.properties![key]"
                :placeholder="$t('ids.apiResource.properties.value')"
                style="width: 400px"
                :disabled="isView"
              />
              <a-button
                type="text"
                status="danger"
                :disabled="isView"
                @click="handleRemoveProperty(key)"
              >
                <template #icon><icon-delete /></template>
              </a-button>
            </a-space>
            <a-button
              v-if="!isView"
              type="dashed"
              long
              @click="handleAddProperty"
            >
              <template #icon><icon-plus /></template>
              {{ $t('ids.apiResource.properties.add') }}
            </a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { Message } from '@arco-design/web-vue';
import { useI18n } from 'vue-i18n';
import type { TableColumnData } from '@arco-design/web-vue/es/table/interface';
import {
  getApiResourcesPaged,
  createApiResource,
  updateApiResource,
  deleteApiResource,
  type ApiResourceDto,
  type ApiResourcePagedQuery,
} from '@/api/ids/api-resource';

const { t } = useI18n();

// 搜索表单
const searchForm = reactive({
  name: '',
  displayName: '',
  enabled: undefined as boolean | undefined,
  scope: '',
});

// 表格数据
const tableData = ref<ApiResourceDto[]>([]);
const loading = ref(false);
const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: true,
  showPageSize: true,
});

// 表格列配置
const columns = computed<TableColumnData[]>(() => [
  {
    title: t('ids.apiResource.name'),
    dataIndex: 'name',
    slotName: 'name',
    width: 150,
  },
  {
    title: t('ids.apiResource.displayName'),
    dataIndex: 'displayName',
    width: 180,
  },
  {
    title: t('ids.apiResource.description'),
    dataIndex: 'description',
    ellipsis: true,
    tooltip: true,
  },
  {
    title: t('ids.apiResource.enabled'),
    dataIndex: 'enabled',
    slotName: 'enabled',
    width: 100,
  },
  {
    title: t('ids.apiResource.scopes'),
    dataIndex: 'scopes',
    slotName: 'scopes',
    width: 200,
  },
  {
    title: t('ids.apiResource.userClaims'),
    dataIndex: 'userClaims',
    slotName: 'userClaims',
    width: 180,
  },
  {
    title: t('ids.apiResource.createdAt'),
    dataIndex: 'createdAt',
    slotName: 'createdAt',
    width: 180,
  },
  {
    title: t('common.operations'),
    slotName: 'operations',
    width: 200,
    fixed: 'right',
  },
]);

// 模态框
const modalVisible = ref(false);
const modalMode = ref<'create' | 'edit' | 'view'>('create');
const modalTitle = computed(() => {
  switch (modalMode.value) {
    case 'create':
      return t('ids.apiResource.create');
    case 'edit':
      return t('ids.apiResource.edit');
    case 'view':
      return t('ids.apiResource.view');
    default:
      return '';
  }
});
const isView = computed(() => modalMode.value === 'view');

// 表单数据
const formRef = ref();
const formData = reactive<ApiResourceDto>({
  name: '',
  displayName: '',
  description: '',
  enabled: true,
  allowedAccessTokenSigningAlgorithms: [],
  showInDiscoveryDocument: true,
  scopes: [],
  userClaims: [],
  properties: {},
});

// 表单验证规则
const formRules = {
  name: [
    { required: true, message: t('ids.apiResource.name.required') },
    { minLength: 2, message: t('ids.apiResource.name.minLength') },
  ],
  displayName: [
    { required: true, message: t('ids.apiResource.displayName.required') },
  ],
};

// 加载数据
const fetchData = async () => {
  loading.value = true;
  try {
    const query: ApiResourcePagedQuery = {
      pageIndex: pagination.current,
      pageSize: pagination.pageSize,
      model: {
        name: searchForm.name || undefined,
        displayName: searchForm.displayName || undefined,
        enabled: searchForm.enabled,
        scope: searchForm.scope || undefined,
      },
    };

    const { data } = await getApiResourcesPaged(query);
    tableData.value = data.items;
    pagination.total = data.totalCount;
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
  searchForm.name = '';
  searchForm.displayName = '';
  searchForm.enabled = undefined;
  searchForm.scope = '';
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

// 创建
const handleCreate = () => {
  modalMode.value = 'create';
  resetFormData();
  modalVisible.value = true;
};

// 查看
const handleView = (record: ApiResourceDto) => {
  modalMode.value = 'view';
  Object.assign(formData, {
    ...record,
    allowedAccessTokenSigningAlgorithms: [
      ...(record.allowedAccessTokenSigningAlgorithms || []),
    ],
    scopes: [...(record.scopes || [])],
    userClaims: [...(record.userClaims || [])],
    properties: record.properties ? { ...record.properties } : {},
  });
  modalVisible.value = true;
};

// 编辑
const handleEdit = (record: ApiResourceDto) => {
  modalMode.value = 'edit';
  Object.assign(formData, {
    ...record,
    allowedAccessTokenSigningAlgorithms: [
      ...(record.allowedAccessTokenSigningAlgorithms || []),
    ],
    scopes: [...(record.scopes || [])],
    userClaims: [...(record.userClaims || [])],
    properties: record.properties ? { ...record.properties } : {},
  });
  modalVisible.value = true;
};

// 删除
const handleDelete = async (record: ApiResourceDto) => {
  try {
    await deleteApiResource(record.id!);
    Message.success(t('common.deleteSuccess'));
    fetchData();
  } catch (error) {
    Message.error(t('common.deleteFailed'));
  }
};

// 模态框确认
const handleModalOk = async () => {
  if (isView.value) {
    modalVisible.value = false;
    return true;
  }

  try {
    await formRef.value.validate();

    if (modalMode.value === 'create') {
      await createApiResource(formData);
      Message.success(t('common.createSuccess'));
    } else {
      await updateApiResource(formData.id!, formData);
      Message.success(t('common.updateSuccess'));
    }

    modalVisible.value = false;
    fetchData();
    return true;
  } catch (error) {
    return false;
  }
};

// 模态框取消
const handleModalCancel = () => {
  modalVisible.value = false;
  formRef.value?.clearValidate();
};

// 重置表单数据
const resetFormData = () => {
  Object.assign(formData, {
    id: undefined,
    name: '',
    displayName: '',
    description: '',
    enabled: true,
    allowedAccessTokenSigningAlgorithms: [],
    showInDiscoveryDocument: true,
    scopes: [],
    userClaims: [],
    properties: {},
  });
  formRef.value?.clearValidate();
};

// 添加 Scope
const handleAddScope = () => {
  formData.scopes.push('');
};

// 删除 Scope
const handleRemoveScope = (index: number) => {
  formData.scopes.splice(index, 1);
};

// 添加 Claim
const handleAddClaim = () => {
  formData.userClaims.push('');
};

// 删除 Claim
const handleRemoveClaim = (index: number) => {
  formData.userClaims.splice(index, 1);
};

// 添加算法
const handleAddAlgorithm = () => {
  formData.allowedAccessTokenSigningAlgorithms.push('');
};

// 删除算法
const handleRemoveAlgorithm = (index: number) => {
  formData.allowedAccessTokenSigningAlgorithms.splice(index, 1);
};

// 添加属性
const handleAddProperty = () => {
  const key = `property_${Date.now()}`;
  formData.properties = formData.properties || {};
  formData.properties[key] = '';
};

// 删除属性
const handleRemoveProperty = (key: string) => {
  if (formData.properties) {
    delete formData.properties[key];
  }
};

// 修改属性键
const handlePropertyKeyChange = (oldKey: string, newKey: string) => {
  if (oldKey === newKey || !formData.properties) return;

  const value = formData.properties[oldKey];
  delete formData.properties[oldKey];
  formData.properties[newKey] = value;
};

// 格式化日期时间
const formatDateTime = (dateStr: string | undefined) => {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleString();
};

// 挂载时加载数据
onMounted(() => {
  fetchData();
});
</script>

<style scoped lang="less">
.container {
  padding: 20px;
}

.form-help-text {
  font-size: 12px;
  color: var(--color-text-3);
  margin-top: 4px;
}
</style>
