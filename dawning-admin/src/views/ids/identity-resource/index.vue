<template>
  <div class="container">
    <Breadcrumb :items="['menu.ids', 'menu.ids.identityResource']" />
    <a-card class="general-card" :title="$t('menu.ids.identityResource')">
      <!-- 搜索表单 -->
      <a-row style="margin-bottom: 16px">
        <a-col :span="24">
          <a-form :model="searchForm" layout="inline">
            <a-form-item field="name" :label="$t('ids.identityResource.name')">
              <a-input
                v-model="searchForm.name"
                :placeholder="$t('ids.identityResource.name.placeholder')"
                allow-clear
                style="width: 200px"
              />
            </a-form-item>
            <a-form-item
              field="displayName"
              :label="$t('ids.identityResource.displayName')"
            >
              <a-input
                v-model="searchForm.displayName"
                :placeholder="
                  $t('ids.identityResource.displayName.placeholder')
                "
                allow-clear
                style="width: 200px"
              />
            </a-form-item>
            <a-form-item
              field="enabled"
              :label="$t('ids.identityResource.enabled')"
            >
              <a-select
                v-model="searchForm.enabled"
                :placeholder="$t('ids.identityResource.enabled.placeholder')"
                allow-clear
                style="width: 120px"
              >
                <a-option :value="true">{{ $t('common.yes') }}</a-option>
                <a-option :value="false">{{ $t('common.no') }}</a-option>
              </a-select>
            </a-form-item>
            <a-form-item
              field="required"
              :label="$t('ids.identityResource.required')"
            >
              <a-select
                v-model="searchForm.required"
                :placeholder="$t('ids.identityResource.required.placeholder')"
                allow-clear
                style="width: 120px"
              >
                <a-option :value="true">{{ $t('common.yes') }}</a-option>
                <a-option :value="false">{{ $t('common.no') }}</a-option>
              </a-select>
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
        <template #required="{ record }">
          <a-tag :color="record.required ? 'red' : 'gray'">
            {{ record.required ? $t('common.yes') : $t('common.no') }}
          </a-tag>
        </template>
        <template #emphasize="{ record }">
          <a-tag :color="record.emphasize ? 'orangered' : 'gray'">
            {{ record.emphasize ? $t('common.yes') : $t('common.no') }}
          </a-tag>
        </template>
        <template #userClaims="{ record }">
          <a-space wrap>
            <a-tag
              v-for="claim in record.userClaims.slice(0, 3)"
              :key="claim"
              color="arcoblue"
            >
              {{ claim }}
            </a-tag>
            <a-tag v-if="record.userClaims.length > 3" color="gray">
              +{{ record.userClaims.length - 3 }}
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
      :width="900"
      @cancel="handleModalCancel"
      @before-ok="handleModalOk"
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
              field="name"
              :label="$t('ids.identityResource.name')"
              required
            >
              <a-input
                v-model="formData.name"
                :placeholder="$t('ids.identityResource.name.placeholder')"
                :disabled="isView"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item
              field="displayName"
              :label="$t('ids.identityResource.displayName')"
              required
            >
              <a-input
                v-model="formData.displayName"
                :placeholder="
                  $t('ids.identityResource.displayName.placeholder')
                "
                :disabled="isView"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-form-item
          field="description"
          :label="$t('ids.identityResource.description')"
        >
          <a-textarea
            v-model="formData.description"
            :placeholder="$t('ids.identityResource.description.placeholder')"
            :auto-size="{ minRows: 2, maxRows: 4 }"
            :disabled="isView"
          />
        </a-form-item>

        <a-row :gutter="16">
          <a-col :span="6">
            <a-form-item
              field="enabled"
              :label="$t('ids.identityResource.enabled')"
            >
              <a-switch v-model="formData.enabled" :disabled="isView" />
              <div class="form-help-text">
                {{ $t('ids.identityResource.enabled.help') }}
              </div>
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item
              field="required"
              :label="$t('ids.identityResource.required')"
            >
              <a-switch v-model="formData.required" :disabled="isView" />
              <div class="form-help-text">
                {{ $t('ids.identityResource.required.help') }}
              </div>
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item
              field="emphasize"
              :label="$t('ids.identityResource.emphasize')"
            >
              <a-switch v-model="formData.emphasize" :disabled="isView" />
              <div class="form-help-text">
                {{ $t('ids.identityResource.emphasize.help') }}
              </div>
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item
              field="showInDiscoveryDocument"
              :label="$t('ids.identityResource.showInDiscoveryDocument')"
            >
              <a-switch
                v-model="formData.showInDiscoveryDocument"
                :disabled="isView"
              />
              <div class="form-help-text">
                {{ $t('ids.identityResource.showInDiscoveryDocument.help') }}
              </div>
            </a-form-item>
          </a-col>
        </a-row>

        <a-divider orientation="left">{{
          $t('ids.identityResource.userClaims')
        }}</a-divider>
        <a-form-item>
          <a-space direction="vertical" fill>
            <a-space v-for="(claim, index) in formData.userClaims" :key="index">
              <a-input
                v-model="formData.userClaims[index]"
                :placeholder="$t('ids.identityResource.userClaims.placeholder')"
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
              {{ $t('ids.identityResource.userClaims.add') }}
            </a-button>
          </a-space>
        </a-form-item>

        <a-divider orientation="left">{{
          $t('ids.identityResource.properties')
        }}</a-divider>
        <a-form-item>
          <a-space direction="vertical" fill>
            <a-space
              v-for="(value, key, index) in formData.properties"
              :key="index"
            >
              <a-input
                :model-value="key"
                :placeholder="$t('ids.identityResource.properties.key')"
                style="width: 200px"
                :disabled="isView"
                @change="(val: string) => handlePropertyKeyChange(key, val)"
              />
              <a-input
                v-model="formData.properties![key]"
                :placeholder="$t('ids.identityResource.properties.value')"
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
              {{ $t('ids.identityResource.properties.add') }}
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
    getIdentityResourcesPaged,
    createIdentityResource,
    updateIdentityResource,
    deleteIdentityResource,
    type IdentityResourceDto,
    type IdentityResourcePagedQuery,
  } from '@/api/ids/identity-resource';

  const { t } = useI18n();

  // 搜索表单
  const searchForm = reactive({
    name: '',
    displayName: '',
    enabled: undefined as boolean | undefined,
    required: undefined as boolean | undefined,
  });

  // 表格数据
  const tableData = ref<IdentityResourceDto[]>([]);
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
      title: t('ids.identityResource.name'),
      dataIndex: 'name',
      slotName: 'name',
      width: 150,
    },
    {
      title: t('ids.identityResource.displayName'),
      dataIndex: 'displayName',
      width: 180,
    },
    {
      title: t('ids.identityResource.description'),
      dataIndex: 'description',
      ellipsis: true,
      tooltip: true,
    },
    {
      title: t('ids.identityResource.enabled'),
      dataIndex: 'enabled',
      slotName: 'enabled',
      width: 100,
    },
    {
      title: t('ids.identityResource.required'),
      dataIndex: 'required',
      slotName: 'required',
      width: 100,
    },
    {
      title: t('ids.identityResource.emphasize'),
      dataIndex: 'emphasize',
      slotName: 'emphasize',
      width: 100,
    },
    {
      title: t('ids.identityResource.userClaims'),
      dataIndex: 'userClaims',
      slotName: 'userClaims',
      width: 250,
    },
    {
      title: t('ids.identityResource.createdAt'),
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
        return t('ids.identityResource.create');
      case 'edit':
        return t('ids.identityResource.edit');
      case 'view':
        return t('ids.identityResource.view');
      default:
        return '';
    }
  });
  const isView = computed(() => modalMode.value === 'view');

  // 表单数据
  const formRef = ref();
  const formData = reactive<IdentityResourceDto>({
    name: '',
    displayName: '',
    description: '',
    enabled: true,
    required: false,
    emphasize: false,
    showInDiscoveryDocument: true,
    userClaims: [],
    properties: {},
  });

  // 表单验证规则
  const formRules = {
    name: [
      { required: true, message: t('ids.identityResource.name.required') },
      { minLength: 2, message: t('ids.identityResource.name.minLength') },
    ],
    displayName: [
      {
        required: true,
        message: t('ids.identityResource.displayName.required'),
      },
    ],
  };

  // 加载数据
  const fetchData = async () => {
    loading.value = true;
    try {
      const query: IdentityResourcePagedQuery = {
        pageIndex: pagination.current,
        pageSize: pagination.pageSize,
        model: {
          name: searchForm.name || undefined,
          displayName: searchForm.displayName || undefined,
          enabled: searchForm.enabled,
          required: searchForm.required,
        },
      };

      const { data } = await getIdentityResourcesPaged(query);
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
    searchForm.required = undefined;
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
  const handleView = (record: IdentityResourceDto) => {
    modalMode.value = 'view';
    Object.assign(formData, {
      ...record,
      userClaims: [...(record.userClaims || [])],
      properties: record.properties ? { ...record.properties } : {},
    });
    modalVisible.value = true;
  };

  // 编辑
  const handleEdit = (record: IdentityResourceDto) => {
    modalMode.value = 'edit';
    Object.assign(formData, {
      ...record,
      userClaims: [...(record.userClaims || [])],
      properties: record.properties ? { ...record.properties } : {},
    });
    modalVisible.value = true;
  };

  // 删除
  const handleDelete = async (record: IdentityResourceDto) => {
    try {
      await deleteIdentityResource(record.id!);
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
        await createIdentityResource(formData);
        Message.success(t('common.createSuccess'));
      } else {
        await updateIdentityResource(formData.id!, formData);
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
      required: false,
      emphasize: false,
      showInDiscoveryDocument: true,
      userClaims: [],
      properties: {},
    });
    formRef.value?.clearValidate();
  };

  // 添加 Claim
  const handleAddClaim = () => {
    formData.userClaims.push('');
  };

  // 删除 Claim
  const handleRemoveClaim = (index: number) => {
    formData.userClaims.splice(index, 1);
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
